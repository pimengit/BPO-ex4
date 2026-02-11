using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BPO_ex4.StationLogic;
using BPO_ex4.Excel;
using BPO_ex4.ViewModels;

namespace BPO_ex4
{
    public delegate void NewTabRequestedHandler(Node node);

    public partial class LogicTabContent : UserControl
    {
        private Context _ctx;
        private SimulationEngine _engine;
        private ExcelSession _session;
        private List<Node> _allNodesCache;

        private Node _currentNode;
        private Stack<string> _history = new Stack<string>();

        public event NewTabRequestedHandler NewTabRequested;
        public event Action GlobalChangeRequested;

        public LogicTabContent()
        {
            InitializeComponent();
        }

        public void Initialize(Context ctx, SimulationEngine engine, ExcelSession session, List<Node> cache, Node startNode)
        {
            _ctx = ctx;
            _engine = engine;
            _session = session;
            _allNodesCache = cache;
            RenderTable(startNode);
        }

        public void RefreshUI()
        {
            if (_currentNode != null) RenderTable(_currentNode);
        }

        private void RenderTable(Node node)
        {
            if (node == null) return;

            if (_currentNode != null && _currentNode.Id != node.Id)
            {
                _history.Push(_currentNode.Id);
                BtnBack.IsEnabled = true;
            }
            _currentNode = node;

            TxtNodeTitle.Text = node.Id;
            TxtNodeDesc.Text = node.Description;
            UpdateStatus();

            if (node.LogicSource is SheetLogic logic)
            {
                string sheetName = GetTypeName(node.Id);
                var truthTable = LogicAnalyzer.GetTruthTable(sheetName);

                var headers = new List<ColumnHeader>();
                for (int i = 1; i < logic.Groups.Length; i++)
                {
                    var group = logic.Groups[i];
                    if (group == null) continue;

                    string opType = LogicAnalyzer.GetGroupType(sheetName, i);
                    if (string.IsNullOrEmpty(opType)) opType = (group.Count > 1) ? "AND" : "V";

                    var wrappers = group.Select(n => new NodeWrapper { LogicNode = _ctx.Get(n.Id) }).ToList();

                    headers.Add(new ColumnHeader
                    {
                        GroupIndex = i,
                        Title = group.Count > 0 ? GetTypeName(group[0].Id) : $"Gr{i}",
                        OperatorType = opType,
                        Nodes = wrappers
                    });
                }
                IcHeaders.ItemsSource = headers;

                var rowVMs = new List<TableRowViewModel>();
                foreach (var row in truthTable)
                {
                    var vm = new TableRowViewModel { RowId = row.RowIndex };
                    bool rowActive = true;

                    foreach (var col in headers)
                    {
                        var cell = new TableCell { Text = "" };
                        if (row.Cells.TryGetValue(col.GroupIndex, out string req))
                        {
                            cell.Text = req;
                            bool currentVal = (col.OperatorType == "OR")
                                ? col.Nodes.Any(n => n.Value)
                                : col.Nodes.All(n => n.Value);
                            if (currentVal != (req == "1")) rowActive = false;
                        }

                        // Таймеры
                        if (col.Nodes != null && col.Nodes.Count > 0)
                        {
                            var inputNode = col.Nodes[0].LogicNode;
                            var totalOn = inputNode.GetTotalOnDelay();
                            var totalOff = inputNode.GetTotalOffDelay();

                            if (totalOn.TotalMilliseconds > 50 || totalOff.TotalMilliseconds > 50)
                            {
                                string info = "";
                                if (totalOn.TotalMilliseconds > 50) info += $"On:{totalOn.TotalSeconds}s ";
                                if (totalOff.TotalMilliseconds > 50) info += $"Off:{totalOff.TotalSeconds}s";
                                cell.TimerInfo = info.Trim();
                            }
                        }
                        vm.Cells.Add(cell);
                    }
                    if (vm.Cells.All(c => c.IsEmpty) && headers.Count > 0) rowActive = false;
                    vm.IsRowActive = rowActive;
                    rowVMs.Add(vm);
                }
                IcRows.ItemsSource = rowVMs;
            }
            else
            {
                IcHeaders.ItemsSource = null;
                IcRows.ItemsSource = null;
            }
        }

        private void UpdateStatus()
        {
            if (_currentNode == null) return;
            bool val = _currentNode.Value;
            TxtNodeValue.Text = val ? "TRUE" : "FALSE";
            TxtNodeValue.Foreground = val ? Brushes.LimeGreen : Brushes.Red;
        }

        // === ВЗАИМОДЕЙСТВИЕ ===
        private void BtnVar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as FrameworkElement).Tag is Node n)
            {
                var realNode = _ctx.Get(n.Id);

                if (e.ChangedButton == MouseButton.Middle)
                {
                    // КОЛЕСИКО -> Новая вкладка
                    NewTabRequested?.Invoke(realNode);
                    e.Handled = true;
                }
                else if (e.ChangedButton == MouseButton.Left)
                {
                    // ЛКМ -> Проваливаемся
                    RenderTable(realNode);
                    e.Handled = true;
                }
                // ПКМ (Right) мы НЕ трогаем, чтобы сработало стандартное ContextMenu из XAML
            }
        }

        // === ОБРАБОТЧИКИ МЕНЮ ===

        // 1. Изменить значение
        private void CtxToggle_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem).Tag is Node n)
            {
                var realNode = _ctx.Get(n.Id); // На всякий случай берем свежую
                ChangeValue(realNode);
            }
        }

        // 2. Редактировать связь
        private void CtxEdit_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem).Tag is Node clickedNode)
            {
                // Ищем, в какой группе и под каким индексом эта нода
                var (groupIdx, inputIdx) = FindInputIndices(_currentNode, clickedNode);

                if (groupIdx != -1)
                {
                    OpenEditWindow(groupIdx, inputIdx);
                }
                else
                {
                    MessageBox.Show("Could not find this input in the current logic structure.");
                }
            }
        }

        // 3. Добавить в группу
        private void CtxAdd_Click(object sender, RoutedEventArgs e)
        {
            // Tag здесь - это GroupIndex (int), который мы привязали в XAML
            if (int.TryParse((sender as MenuItem).Tag?.ToString(), out int groupIdx))
            {
                OpenEditWindow(groupIdx, -1);
            }
        }

        // === ХЕЛПЕРЫ ===

        private void ChangeValue(Node n)
        {
            if (n.Id.StartsWith("CONST")) return;
            _engine.InjectChange(n, !n.Value);
            GlobalChangeRequested?.Invoke();
        }

        // Метод поиска индексов (который был раньше в MainWindow)
        private (int groupIdx, int inputIdx) FindInputIndices(Node center, Node input)
        {
            if (center.LogicSource is SheetLogic logic && logic.Groups != null)
            {
                for (int i = 1; i < logic.Groups.Length; i++)
                {
                    var group = logic.Groups[i];
                    if (group == null) continue;
                    for (int k = 0; k < group.Count; k++)
                    {
                        if (group[k].Id == input.Id) return (i, k);
                    }
                }
            }
            return (-1, -1);
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (_history.Count > 0)
            {
                string prevId = _history.Pop();
                RenderTable(_ctx.Get(prevId));
                if (_history.Count > 0 && _history.Peek() == prevId) _history.Pop();
                BtnBack.IsEnabled = _history.Count > 0;
            }
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int g) OpenEditWindow(g, -1);
        }

        // === ИСПРАВЛЕННЫЙ МЕТОД РЕДАКТИРОВАНИЯ ===
        private void OpenEditWindow(int groupIdx, int inputIdx)
        {
            if (!_session.IsLoaded) { MessageBox.Show("Load file first!"); return; }

            var win = new EditNodeWindow(_currentNode, groupIdx, inputIdx, _allNodesCache);
            if (win.ShowDialog() == true)
            {
                try
                {
                    var selectedNode = _ctx.Get(win.ResultSourceNode.Id);

                    // 1. Пишем в память Excel (БЫСТРО)
                    if (inputIdx == -1)
                        _session.AddInputInMemory(win.TargetSheetName, win.TargetObjectIndex, groupIdx, selectedNode);
                    else
                        _session.UpdateCellInMemory(win.TargetSheetName, win.TargetObjectIndex, groupIdx, inputIdx, selectedNode);

                    // 2. Пишем в память Графа (БЫСТРО)
                    if (inputIdx == -1)
                        LogicPatcher.AddInputToRuntime(_currentNode, groupIdx, selectedNode);
                    else
                        LogicPatcher.UpdateInputInRuntime(_currentNode, groupIdx, inputIdx, selectedNode);

                    // 3. УВЕДОМЛЕНИЕ О ИЗМЕНЕНИЯХ (НО НЕ СОХРАНЕНИЕ НА ДИСК!)
                    GlobalChangeRequested?.Invoke();

                    // УБРАНО: _session.Save(); -> Теперь это происходит только в MainWindow по кнопке или таймеру
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            }
        }

        private string GetTypeName(string id) => id.Contains('[') ? id.Substring(0, id.IndexOf('[')) : id;
    }
}