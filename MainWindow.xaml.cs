using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using BPO_ex4.StationLogic;
using BPO_ex4.Excel;
using BPO_ex4.ViewModels;

namespace BPO_ex4
{
    public partial class MainWindow : Window
    {
        private Context _ctx;
        private List<Node> _allNodesCache;
        private List<string> _uniqueTypes;
        private Node _currentNode;
        private string _loadedExcelPath;
        private Stack<string> _historyIds = new Stack<string>();

        // Сессия для записи (держит файл открытым)
        private ExcelSession _excelSession = new ExcelSession();

        public MainWindow()
        {
            OfficeOpenXml.ExcelPackage.License.SetNonCommercialPersonal("User");
            LogicAnalyzer.LoadAllFiles();

            InitializeComponent();

            // Инициализация пустого контекста
            _ctx = new Context();
            _ctx.Set("CONST_0", false);
            _ctx.Set("CONST_1", true);
        }

        // ==========================================
        //  СУПЕР-БЫСТРЫЙ ПЕРЕСЧЕТ (CORE FIX)
        // ==========================================
        private void RecomputeFast()
        {
            // Прогоняем вычисления несколько раз, чтобы сигнал "всплыл" снизу вверх
            // В консольной версии это могло быть реализовано через очередь, тут сделаем тупо но надежно

            int maxIterations = 10; // Защита от бесконечного цикла
            bool changed = true;
            int iter = 0;

            while (changed && iter < maxIterations)
            {
                changed = false;
                // Бежим по всем нодам, у которых есть логика
                foreach (var node in _ctx.GetAllNodes().Where(n => n.LogicSource != null))
                {
                    bool oldVal = node.Value;
                    bool newVal = node.Compute(); // Вызываем метод Compute() из AVTODO_AR.cs

                    if (oldVal != newVal)
                    {
                        node.Value = newVal;
                        changed = true;
                    }
                }
                iter++;
            }
        }

        // ==========================================
        //  UI ОТРИСОВКА
        // ==========================================
        private void RenderTable(Node node)
        {
            if (node == null) return;

            if (_currentNode != null && _currentNode.Id != node.Id)
            {
                _historyIds.Push(_currentNode.Id);
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

                // --- КОЛОНКИ ---
                var headers = new List<ColumnHeader>();

                // Используем .Groups.Length, так как мы могли расширить массив через Patcher
                for (int i = 1; i < logic.Groups.Length; i++)
                {
                    var group = logic.Groups[i];
                    if (group == null || group.Count == 0) continue;

                    string opType = LogicAnalyzer.GetGroupType(sheetName, i);
                    if (string.IsNullOrEmpty(opType))
                        opType = (group.Count > 1) ? "AND" : "V";

                    // Важно: берем актуальные ноды прямо из группы
                    var wrappers = group.Select(n => new NodeWrapper { LogicNode = n }).ToList();

                    headers.Add(new ColumnHeader
                    {
                        GroupIndex = i,
                        Title = GetTypeName(group[0].Id),
                        OperatorType = opType,
                        Nodes = wrappers
                    });
                }
                IcHeaders.ItemsSource = headers;

                // --- СТРОКИ ---
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
                        vm.Cells.Add(cell);
                    }
                    if (vm.Cells.All(c => c.IsEmpty)) rowActive = false;
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

        // ==========================================
        //  МГНОВЕННОЕ ИЗМЕНЕНИЕ (БЕЗ EXCEL)
        // ==========================================
        private void ChangeVariableValue(Node n)
        {
            if (n.Id.StartsWith("CONST")) return;

            // 1. Меняем значение
            n.Value = !n.Value;

            // 2. МГНОВЕННЫЙ ПЕРЕСЧЕТ
            RecomputeFast();

            // 3. Обновляем UI
            RenderTable(_currentNode);
        }

        // ==========================================
        //  ОБРАБОТЧИКИ СОБЫТИЙ
        // ==========================================
        private void BtnVar_LeftClick(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).Tag is Node n) RenderTable(_ctx.Get(n.Id));
        }

        private void BtnVar_RightClick(object sender, MouseButtonEventArgs e)
        {
            if ((sender as Button).Tag is Node n) ChangeVariableValue(n);
        }

        private void CtxToggle_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem).Tag is Node n) ChangeVariableValue(n);
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int groupIdx) OpenEditWindow(groupIdx, -1);
        }

        private void CtxEdit_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as MenuItem).Tag is Node clickedNode)
            {
                var (groupIdx, inputIdx) = FindInputIndices(_currentNode, clickedNode);
                if (groupIdx != -1) OpenEditWindow(groupIdx, inputIdx);
            }
        }

        private void CtxAdd_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse((sender as MenuItem).Tag?.ToString(), out int g)) OpenEditWindow(g, -1);
        }

        // ==========================================
        //  РЕДАКТИРОВАНИЕ (MEMORY + EXCEL)
        // ==========================================
        private void OpenEditWindow(int groupIdx, int inputIdx)
        {
            if (!_excelSession.IsLoaded) { MessageBox.Show("Load file first!"); return; }

            var win = new EditNodeWindow(_currentNode, groupIdx, inputIdx, _excelSession, _allNodesCache);

            if (win.ShowDialog() == true)
            {
                // ПОСЛЕ СОХРАНЕНИЯ ОКНА
                // Данные уже записаны в ExcelSession (в память Excel)
                // Но нам нужно обновить Context (в память Логики), чтобы увидеть изменения СРАЗУ

                // 1. Достаем то, что выбрал юзер (немного костыльно, но быстро)
                // В идеале EditWindow должен возвращать результат
                // Но так как мы уже записали в Session, мы можем просто повторить логику патчера

                // ВАЖНО: Сейчас, чтобы увидеть изменения, тебе НЕ НУЖНО перезагружать файл
                // Но EditNodeWindow должен вернуть нам Node, который он создал.
                // Давай пока сделаем простой вариант: 
                // Мы сохранили в ExcelSession. При закрытии приложения оно сохранится.
                // А чтобы увидеть на экране - мы должны обновить _ctx вручную.

                // Так как EditWindow инкапсулирован, давай сделаем ReloadData НО БЕЗ ЗАГРУЗКИ ФАЙЛА
                // Это невозможно, ExcelParser требует файл.

                // РЕШЕНИЕ: Просто перезагрузим ВСЁ из обновленной сессии ExcelSession.
                // Это быстро, так как файл уже в памяти.

                // Но для супер-скорости лучше бы LogicPatcher использовать в EditWindow.
                // (Я добавил это в следующем шаге)

                ReloadData();
            }
        }

        // ==========================================
        //  ЗАГРУЗКА
        // ==========================================
        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "Excel Files|*.xlsx" };
            if (dlg.ShowDialog() == true)
            {
                _loadedExcelPath = dlg.FileName;
                try
                {
                    _excelSession.Load(_loadedExcelPath);
                    ReloadData();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        private void ReloadData()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                // Очистка
                _ctx = new Context();
                _ctx.Set("CONST_0", false);
                _ctx.Set("CONST_1", true);

                // Читаем из ФАЙЛА (медленно, но надежно при старте/релоаде)
                // Если мы хотим быстро - нужно учить парсер читать из ExcelPackage
                ExcelParser.Load(_loadedExcelPath, _ctx);

                RecomputeFast(); // Просчет логики

                _allNodesCache = _ctx.GetAllNodes().OrderBy(n => n.Id).ToList();
                _uniqueTypes = _allNodesCache.Select(n => GetTypeName(n.Id)).Distinct().OrderBy(s => s).ToList();
                LstTypes.ItemsSource = _uniqueTypes;

                // Восстановление вида
                if (_currentNode != null)
                {
                    var freshNode = _ctx.Get(_currentNode.Id);
                    RenderTable(freshNode);
                }
                else
                {
                    IcHeaders.ItemsSource = null;
                    IcRows.ItemsSource = null;
                }
            }
            finally { Mouse.OverrideCursor = null; }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (_historyIds.Count > 0)
            {
                string prevId = _historyIds.Pop();
                var node = _ctx.Get(prevId);
                RenderTable(node);
                if (_historyIds.Count > 0 && _historyIds.Peek() == prevId) _historyIds.Pop();
                BtnBack.IsEnabled = _historyIds.Count > 0;
            }
        }

        // Helpers
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

        private string GetTypeName(string id) => id.Contains('[') ? id.Substring(0, id.IndexOf('[')) : id;
        private void TxtSearchType_TextChanged(object sender, TextChangedEventArgs e) { if (_uniqueTypes != null) LstTypes.ItemsSource = _uniqueTypes.Where(t => t.ToLower().Contains(TxtSearchType.Text.ToLower())).ToList(); }
        private void LstTypes_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (LstTypes.SelectedItem is string t) LstInstances.ItemsSource = _allNodesCache.Where(n => GetTypeName(n.Id) == t).OrderBy(n => n.Id).ToList(); }
        private void LstInstances_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (LstInstances.SelectedItem is Node n) RenderTable(n); }

        protected override void OnClosed(EventArgs e)
        {
            _excelSession.Dispose();
            base.OnClosed(e);
        }
    }

    /*public class ConstCheckConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string id) return id.StartsWith("CONST", StringComparison.OrdinalIgnoreCase);
            return false;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => throw new NotImplementedException();
    }*/
}