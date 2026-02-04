using BPO_ex4.Excel;
using BPO_ex4.Excel;
// Ваши пространства имен (из старых файлов)
using BPO_ex4.StationLogic;
using BPO_ex4.StationLogic;
using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
// Используем псевдоним для MSAGL, чтобы избежать путаницы
using Msagl = Microsoft.Msagl.Drawing;

namespace BPO_ex4
{
    public partial class MainWindow : Window
    {
        private Context _ctx;
        private List<BPO_ex4.StationLogic.Node> _allNodesCache;
        private List<string> _uniqueTypes;

        // Вьювер графа
        private Microsoft.Msagl.WpfGraphControl.GraphViewer _graphViewer = new Microsoft.Msagl.WpfGraphControl.GraphViewer();

        public MainWindow()
        {
            ExcelPackage.License.SetNonCommercialPersonal("<Your Name>");
            InitializeComponent();

            // Настраиваем MSAGL
            _graphViewer.BindToPanel(GraphContainer);
            _graphViewer.RunLayoutAsync = true;
            // _graphViewer.ToolBarIsVisible = true; // <-- ЭТА СТРОКА УДАЛЕНА, так как её нет в WPF версии

            _ctx = new Context();
            _ctx.Set("CONST_0", false);
            _ctx.Set("CONST_1", true);
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "Excel Files|*.xlsx" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    ExcelParser.Load(dlg.FileName, _ctx);
                    _ctx.RecomputeAll();

                    _allNodesCache = _ctx.GetAllNodes().OrderBy(n => n.Id).ToList();
                    _uniqueTypes = _allNodesCache.Select(n => GetTypeName(n.Id)).Distinct().OrderBy(s => s).ToList();

                    LstTypes.ItemsSource = _uniqueTypes;
                    Title = $"Project Loaded: {_allNodesCache.Count} vars";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
        }

        private string GetTypeName(string id)
        {
            int idx = id.IndexOf('[');
            return idx > 0 ? id.Substring(0, idx) : id;
        }

        private void TxtSearchType_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_uniqueTypes == null) return;
            string filter = TxtSearchType.Text.ToLower();
            LstTypes.ItemsSource = _uniqueTypes.Where(t => t.ToLower().Contains(filter)).ToList();
        }

        private void LstTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstTypes.SelectedItem is string selectedType)
            {
                var instances = _allNodesCache
                    .Where(n => GetTypeName(n.Id) == selectedType)
                    .OrderBy(n => n.Id)
                    .ToList();
                LstInstances.ItemsSource = instances;
            }
        }

        // ==========================================
        // ПОСТРОЕНИЕ ГРАФА
        // ==========================================
        private void LstInstances_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstInstances.SelectedItem is BPO_ex4.StationLogic.Node logicNode)
            {
                BuildMsaglGraph(logicNode);
            }
        }

        private void BuildMsaglGraph(BPO_ex4.StationLogic.Node centerNode)
        {
            // 1. Создаем граф (Явно используем Msagl.Graph)
            var graph = new Msagl.Graph("logic");

            // 2. Центральный узел
            var centerNodeUi = graph.AddNode(centerNode.Id);
            centerNodeUi.Attr.FillColor = Msagl.Color.LightBlue;
            centerNodeUi.Attr.Shape = Msagl.Shape.Box;
            centerNodeUi.Attr.LineWidth = 2;

            // 3. Входы
            if (centerNode.LogicSource is SheetLogic logic)
            {
                var groups = logic.Groups;
                for (int i = 1; i < groups.Length; i++)
                {
                    var group = groups[i];
                    if (group == null || group.Count == 0) continue;

                    // Целевая нода для текущей группы
                    Msagl.Node targetForInput = centerNodeUi;

                    // Если это группа "OR" (> 1 элемента)
                    if (group.Count > 1)
                    {
                        var orId = $"{centerNode.Id}_OR_{i}";
                        var orNode = graph.AddNode(orId);
                        orNode.LabelText = "OR";
                        orNode.Attr.Shape = Msagl.Shape.Circle;
                        orNode.Attr.FillColor = Msagl.Color.Orange;

                        // Связь OR -> Center
                        graph.AddEdge(orId, centerNode.Id);

                        targetForInput = orNode;
                    }

                    // Добавляем входы
                    foreach (var inputLogicNode in group)
                    {
                        var inputNodeUi = graph.AddNode(inputLogicNode.Id);

                        inputNodeUi.Attr.FillColor = inputLogicNode.Value ? Msagl.Color.LightGreen : Msagl.Color.White;
                        inputNodeUi.Attr.Shape = Msagl.Shape.Box;

                        graph.AddEdge(inputLogicNode.Id, targetForInput.Id);
                    }
                }
            }

            // 4. Отображаем
            _graphViewer.Graph = graph;
        }
    }
}