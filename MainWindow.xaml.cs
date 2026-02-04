using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

// Твои пространства имен (BPO_ex4)
// Убедись, что Node и Context лежат именно в BPO_ex4.StationLogic, 
// если они остались в BPO_ex2, поменяй using ниже.
using BPO_ex4.StationLogic;
using BPO_ex4.Excel;

// Псевдонимы для MSAGL
using Msagl = Microsoft.Msagl.Drawing;
using WpfCtrl = Microsoft.Msagl.WpfGraphControl;

namespace BPO_ex4
{
    public partial class MainWindow : Window
    {
        private Context _ctx;
        private List<Node> _allNodesCache;
        private List<string> _uniqueTypes;

        // Вьювер графа
        private WpfCtrl.GraphViewer _graphViewer = new WpfCtrl.GraphViewer();

        // Текущая центральная нода
        private Node _currentNode;

        // Путь к файлу для сохранения
        private string _loadedExcelPath;

        // История навигации (Стек)
        private Stack<Node> _history = new Stack<Node>();
        private bool _isNavigatingBack = false;

        public MainWindow()
        {
            // Лицензия EPPlus
            OfficeOpenXml.ExcelPackage.License.SetNonCommercialPersonal("User");

            // 1. Загружаем кэш формул из .cs файлов (чтобы видеть OR/AND/!)
            LogicAnalyzer.LoadAllFiles();

            InitializeComponent();

            // 2. Настройка MSAGL
            _graphViewer.BindToPanel(GraphContainer);
            _graphViewer.RunLayoutAsync = true;
            _graphViewer.MouseDown += GraphViewer_MouseDown; // Подписка на клики

            // 3. Инициализация контекста
            _ctx = new Context();
            _ctx.Set("CONST_0", false);
            _ctx.Set("CONST_1", true);
        }

        // ==========================================
        //  UI: КНОПКИ И СПИСКИ
        // ==========================================

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "Excel Files|*.xlsx" };
            if (dlg.ShowDialog() == true)
            {
                _loadedExcelPath = dlg.FileName;
                ReloadData();
            }
        }

        private void ReloadData()
        {
            try
            {
                // Полная перезагрузка контекста из файла
                // (Чтобы подтянуть изменения после редактирования)
                var newCtx = new Context();
                newCtx.Set("CONST_0", false);
                newCtx.Set("CONST_1", true);

                ExcelParser.Load(_loadedExcelPath, newCtx);
                newCtx.RecomputeAll();

                _ctx = newCtx; // Подменяем контекст

                _allNodesCache = _ctx.GetAllNodes().OrderBy(n => n.Id).ToList();
                _uniqueTypes = _allNodesCache.Select(n => GetTypeName(n.Id)).Distinct().OrderBy(s => s).ToList();

                LstTypes.ItemsSource = _uniqueTypes;

                // Если мы смотрели на какую-то ноду, нужно найти её новую версию в новом контексте
                if (_currentNode != null)
                {
                    _currentNode = _ctx.Get(_currentNode.Id);
                    BuildMsaglGraph(_currentNode);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading data: {ex.Message}");
            }
        }

        // Кнопка НАЗАД
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (_history.Count > 0)
            {
                var prevNode = _history.Pop();

                _isNavigatingBack = true; // Блокируем запись в историю

                // Находим актуальную версию ноды в текущем контексте
                var actualNode = _ctx.Get(prevNode.Id);
                BuildMsaglGraph(actualNode);

                _isNavigatingBack = false;

                BtnBack.IsEnabled = _history.Count > 0;
            }
        }

        // Вспомогательные методы списков
        private string GetTypeName(string id) => id.Contains('[') ? id.Substring(0, id.IndexOf('[')) : id;

        private void TxtSearchType_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_uniqueTypes != null)
                LstTypes.ItemsSource = _uniqueTypes.Where(t => t.ToLower().Contains(TxtSearchType.Text.ToLower())).ToList();
        }

        private void LstTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstTypes.SelectedItem is string t)
                LstInstances.ItemsSource = _allNodesCache.Where(n => GetTypeName(n.Id) == t).OrderBy(n => n.Id).ToList();
        }

        private void LstInstances_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstInstances.SelectedItem is Node n)
                BuildMsaglGraph(n);
        }

        // ==========================================
        //  ПОСТРОЕНИЕ ГРАФА (С АНАЛИЗОМ КОДА)
        // ==========================================
        private void BuildMsaglGraph(Node centerNode)
        {
            // История
            if (!_isNavigatingBack && _currentNode != null && _currentNode.Id != centerNode.Id)
            {
                _history.Push(_currentNode);
                BtnBack.IsEnabled = true;
            }

            _currentNode = centerNode;

            var graph = new Msagl.Graph("logic");

            // 1. Рисуем ЦЕНТР
            var centerUi = graph.AddNode(centerNode.Id);
            centerUi.Attr.FillColor = Msagl.Color.LightBlue;
            centerUi.Attr.LineWidth = 3;
            centerUi.Attr.Shape = Msagl.Shape.Box;

            // 2. Рисуем ВХОДЫ
            if (centerNode.LogicSource is SheetLogic logic)
            {
                // Имя типа для парсера (например "AVTODO_AR")
                string sheetType = GetTypeName(centerNode.Id);
                var groups = logic.Groups;

                for (int i = 1; i < groups.Length; i++)
                {
                    var group = groups[i];
                    if (group == null || group.Count == 0) continue;

                    // Анализируем код C#: какой оператор (OR/AND) и есть ли инверсия (!)
                    var info = LogicAnalyzer.AnalyzeGroup(sheetType, i);

                    Msagl.Node targetForInput = centerUi;

                    // --- РИСУЕМ ГРУППОВОЙ УЗЕЛ (КРУЖОК) ---
                    // Рисуем кружок, если это OR/AND или если в группе > 1 входа
                    if (info.OperatorType == "OR" || info.OperatorType == "AND" || group.Count > 1)
                    {
                        var opId = $"{centerNode.Id}_GR_{i}";
                        var opNode = graph.AddNode(opId);

                        // Текст: берем из кода (OR/AND) или по умолчанию "&"
                        string label = (info.OperatorType != "UNK" && info.OperatorType != "V") ? info.OperatorType : "&";
                        opNode.LabelText = label;
                        opNode.Attr.Shape = Msagl.Shape.Circle;

                        // Цвет кружка (примерный расчет)
                        bool isGreen = false;
                        if (label.Contains("OR")) isGreen = group.Any(n => n.Value);
                        else isGreen = group.All(n => n.Value); // AND

                        opNode.Attr.FillColor = isGreen ? Msagl.Color.LightGreen : Msagl.Color.White;

                        // Связь КРУЖОК -> ЦЕНТР
                        var edge = graph.AddEdge(opId, centerNode.Id);

                        // Если в коде !OR(...), красим линию в красный
                        ApplyEdgeStyle(edge, info.IsInverted);

                        // Теперь входы цепляются к этому кружку
                        targetForInput = opNode;
                    }
                    else
                    {
                        // Одиночный вход V(x). Инверсию нарисуем на линии от входа.
                    }

                    // --- РИСУЕМ ВХОДЫ ---
                    foreach (var inputLogicNode in group)
                    {
                        var inputUi = graph.AddNode(inputLogicNode.Id);
                        inputUi.Attr.FillColor = inputLogicNode.Value ? Msagl.Color.LightGreen : Msagl.Color.White;
                        inputUi.Attr.Shape = Msagl.Shape.Box;
                        inputUi.Attr.XRadius = 3; inputUi.Attr.YRadius = 3;

                        var edge = graph.AddEdge(inputLogicNode.Id, targetForInput.Id);

                        // Если это одиночный вход V(x) и он инвертирован !V(x), красим красным здесь
                        // (Если это группа, то инверсия уже нарисована на выходе из кружка)
                        if (targetForInput == centerUi && info.IsInverted)
                        {
                            ApplyEdgeStyle(edge, true);
                        }
                    }
                }
            }

            _graphViewer.Graph = graph;
        }

        private void ApplyEdgeStyle(Msagl.Edge edge, bool inverted)
        {
            if (inverted)
            {
                edge.Attr.Color = Msagl.Color.Red;
                edge.Attr.LineWidth = 2;
                edge.LabelText = "!"; // Значок инверсии
            }
            else
            {
                edge.Attr.Color = Msagl.Color.Black;
                edge.Attr.LineWidth = 1;
            }
        }

        // ==========================================
        //  ИНТЕРАКТИВ И РЕДАКТОР (ПКМ, ЛКМ)
        // ==========================================

        private void GraphViewer_MouseDown(object sender, Msagl.MsaglMouseEventArgs e)
        {
            var obj = _graphViewer.ObjectUnderMouseCursor;

            // Проверка, что кликнули по узлу
            if (obj?.DrawingObject is Msagl.Node drawingNode)
            {
                var nodeId = drawingNode.LabelText;

                // Игнорируем служебные узлы (OR, AND, &)
                if (nodeId.Contains("_GR_") || nodeId == "OR" || nodeId == "AND" || nodeId == "&") return;

                // Получаем объект логики
                var logicNode = _ctx.Get(nodeId);

                // ЛКМ Двойной клик -> Провалиться (Drill Down)
                if (e.LeftButtonIsPressed && e.Clicks == 2)
                {
                    BuildMsaglGraph(logicNode);
                    e.Handled = true;
                }
                // ПКМ -> Контекстное меню
                else if (e.RightButtonIsPressed)
                {
                    // Определяем, кликнули мы по ЦЕНТРУ или по ВХОДУ
                    bool isInput = (logicNode != _currentNode);

                    // Запускаем меню в UI потоке
                    Dispatcher.Invoke(() => OpenContextMenu(logicNode, isInput));
                    e.Handled = true;
                }
            }
        }

        private void OpenContextMenu(Node node, bool isInput)
        {
            ContextMenu cm = new ContextMenu();

            // 1. Toggle Value (Эмуляция)
            MenuItem itemToggle = new MenuItem { Header = $"Toggle Value (Current: {node.Value})" };
            itemToggle.Click += (s, a) =>
            {
                _ctx.Set(node.Id, !node.Value);
                _ctx.RecomputeAll();

                // Небольшая пауза для таймеров (костыль, но рабочий)
                System.Threading.Thread.Sleep(20);

                // Перерисовка
                if (_currentNode != null) BuildMsaglGraph(_currentNode);
            };
            cm.Items.Add(itemToggle);

            // 2. Edit Connection (Только если это вход)
            if (isInput)
            {
                MenuItem itemEdit = new MenuItem { Header = "🖊 Edit This Input (Excel)..." };
                itemEdit.Click += (s, a) =>
                {
                    if (string.IsNullOrEmpty(_loadedExcelPath)) { MessageBox.Show("Please save/load Excel file first."); return; }

                    // Нам нужно найти, в какой группе и под каким индексом этот вход сидит у текущей ноды
                    // Это нужно для передачи в EditNodeWindow
                    var (groupIdx, inputIdx) = FindInputIndices(_currentNode, node);

                    if (groupIdx != -1)
                    {
                        var win = new EditNodeWindow(_currentNode, groupIdx, inputIdx, _loadedExcelPath);
                        if (win.ShowDialog() == true)
                        {
                            ReloadData(); // Если сохранили - перезагружаем всё
                        }
                    }
                    else
                    {
                        MessageBox.Show("Error: Could not find this input in the logic structure.");
                    }
                };
                cm.Items.Add(itemEdit);
            }

            // 3. Add New Input (Всегда доступно для центральной ноды)
            MenuItem itemAdd = new MenuItem { Header = "➕ Add New Input to Group..." };
            // Добавляем подменю для выбора группы
            if (_currentNode.LogicSource is SheetLogic logic)
            {
                for (int i = 1; i < logic.Groups.Length; i++)
                {
                    int g = i; // замыкание
                    var groupItem = new MenuItem { Header = $"Group {g}" };
                    groupItem.Click += (s, a) =>
                    {
                        // -1 означает добавление нового
                        var win = new EditNodeWindow(_currentNode, g, -1, _loadedExcelPath);
                        if (win.ShowDialog() == true)
                        {
                            ReloadData();
                        }
                    };
                    itemAdd.Items.Add(groupItem);
                }
            }
            cm.Items.Add(itemAdd);

            cm.IsOpen = true;
        }

        // Поиск индекса входа (в какой группе сидит clickedNode?)
        private (int groupIdx, int inputIdx) FindInputIndices(Node center, Node input)
        {
            if (center.LogicSource is SheetLogic logic)
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
    }
}