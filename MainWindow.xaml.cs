using BPO_ex4.Excel;
using BPO_ex4.StationLogic;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BPO_ex4
{
    public partial class MainWindow : Window
    {
        private Context _ctx;
        private SimulationEngine _engine;
        private ExcelSession _excelSession = new ExcelSession();
        private List<Node> _allNodesCache;
        private List<string> _uniqueTypes;
        private string _loadedExcelPath;

        public MainWindow()
        {
            OfficeOpenXml.ExcelPackage.License.SetNonCommercialPersonal("User");
            LogicAnalyzer.LoadAllFiles();
            InitializeComponent();
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "Excel Files|*.xlsm;*.xlsx" };
            if (dlg.ShowDialog() == true)
            {
                _loadedExcelPath = dlg.FileName;
                _excelSession.Load(_loadedExcelPath);
                ReloadData();
            }
        }

        private void ReloadData()
        {
            _ctx = new Context();
            _ctx.Set("CONST_0", false);
            _ctx.Set("CONST_1", true);

            // 1. Загружаем данные
            ExcelParser.Load(_loadedExcelPath, _ctx);

            ApplyHardcodedDefaults(_ctx);
            // 2. Инициализируем движок
            _engine = new SimulationEngine();
            // Если сработал таймер -> обновляем все вкладки
            _engine.UIUpdateRequested += OnGlobalChange;

            // 3. Подписываем ноды на таймеры
            foreach (var node in _ctx.GetAllNodes())
            {
                node.DelayedUpdateReady += _engine.OnDelayedUpdateReady;
                if (node.LogicSource != null) node.Value = node.Compute();
            }

            // 4. Заполняем списки
            _allNodesCache = _ctx.GetAllNodes().OrderBy(n => n.Id).ToList();
            _uniqueTypes = _allNodesCache.Select(n => GetTypeName(n.Id)).Distinct().OrderBy(s => s).ToList();
            LstTypes.ItemsSource = _uniqueTypes;

            // Очищаем вкладки
            MainTabs.Items.Clear();
        }

        // === МЕТОДЫ ДЛЯ ВКЛАДОК ===

        private void OpenNewTab(Node node)
        {
            if (node == null) return;

            // Проверка на дубликаты (по желанию раскомментируйте return)
            foreach (TabItem item in MainTabs.Items)
            {
                // Проверяем по заголовку, так как Name у UserControl может быть не задан
                var headerPanel = item.Header as StackPanel;
                var textBlock = headerPanel?.Children.OfType<TextBlock>().FirstOrDefault();

                if (textBlock != null && textBlock.Text == node.Id)
                {
                    MainTabs.SelectedItem = item;
                    // return; 
                }
            }

            // 1. Создаем контент
            var content = new LogicTabContent();
            content.Initialize(_ctx, _engine, _excelSession, _allNodesCache, node);
            content.NewTabRequested += OpenNewTab;
            content.GlobalChangeRequested += OnGlobalChange;

            // 2. Создаем саму вкладку
            var tab = new TabItem();

            // === 3. СОЗДАЕМ ЗАГОЛОВОК С КРЕСТИКОМ (ВРУЧНУЮ) ===
            var headerStack = new StackPanel { Orientation = Orientation.Horizontal };

            // Текст названия переменной
            var txtTitle = new TextBlock
            {
                Text = node.Id,
                Margin = new Thickness(0, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            // Кнопка "X"
            var btnClose = new Button
            {
                Content = "x",
                Width = 16,
                Height = 16,
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Red,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand,
                VerticalAlignment = VerticalAlignment.Center,
                // Самое важное: сохраняем ссылку на вкладку в Tag, чтобы потом знать, кого закрывать
                Tag = tab
            };

            // Подписываемся на клик (метод BtnCloseTab_Click у вас уже есть в коде)
            btnClose.Click += BtnCloseTab_Click;

            // Собираем всё вместе
            headerStack.Children.Add(txtTitle);
            headerStack.Children.Add(btnClose);

            // Присваиваем заголовок и контент
            tab.Header = headerStack;
            tab.Content = content;
            tab.Tag = content; // Tag самой вкладки хранит её содержимое (для поиска)

            MainTabs.Items.Add(tab);
            MainTabs.SelectedItem = tab;
        }

        // Метод для жесткой настройки начальных значений
        private void ApplyHardcodedDefaults(Context ctx)
        {
            // 1. Хардкод конкретных имен (если они точно есть)
            //var specificNames = new List<string> { "NV", "K_1", "POWER_ON" };
            /*/foreach (var name in specificNames)
            {
                // Тут можно использовать Get, если вы уверены на 100%
                // Но лучше проверить через Contains, если он есть, или просто Get, так как их мало
                var node = ctx.Get(name);
                node.Value = true;
            }*/

            // 2. МАССОВОЕ ВКЛЮЧЕНИЕ ПО МАСКЕ (Без создания мусора)
            // Мы бежим только по тем нодам, которые реально загрузились из файла
            foreach (var node in ctx.GetAllNodes())
            {
                // Проверяем, начинается ли имя на SECT_IN
                if (node.Id.StartsWith("SECT_IN"))
                {
                    // (Опционально) Если нужно проверить индексы [i:c]
                    // Но обычно достаточно просто включить все SECT_IN
                    node.Value = true;
                }

                // Пример для другой группы
                if (node.Id.StartsWith("GEN_OUT"))
                {
                    node.Value = true;
                }
            }

            


        }
        
        private void OnGlobalChange()
        {
            // Проходим по всем открытым вкладкам и просим обновиться
            foreach (TabItem tab in MainTabs.Items)
            {
                if (tab.Content is LogicTabContent content)
                {
                    content.RefreshUI();
                }
            }
        }

        private void BtnOpenScheme_Click(object sender, RoutedEventArgs e)
        {
            if (_ctx == null) return;

            // Передаем _ctx И _engine
            var schemeWin = new StationViewWindow(_ctx, _engine);
            schemeWin.Show();
        }


        private void BtnCloseTab_Click(object sender, RoutedEventArgs e)
        {
            // Кнопка закрытия находится внутри DataTemplate заголовка,
            // поэтому Tag мы привязали к самой вкладке (TabItem)
            if ((sender as Button).Tag is TabItem tab)
            {
                MainTabs.Items.Remove(tab);
            }
        }

        // === ВЫБОР ИЗ СПИСКА ===
        private void LstInstances_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstInstances.SelectedItem is Node n)
            {
                // Логика: если вкладки есть, обновляем текущую. Если нет - создаем новую.
                if (MainTabs.SelectedItem is TabItem tab && tab.Content is LogicTabContent content)
                {
                    content.Initialize(_ctx, _engine, _excelSession, _allNodesCache, n);
                    tab.Header = n.Id; // Меняем заголовок
                }
                else
                {
                    OpenNewTab(n);
                }
            }
        }

        // === ХЕЛПЕРЫ ===
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                _excelSession.Save();
                MessageBox.Show("Saved!");
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
            finally { Mouse.OverrideCursor = null; }
        }

        private void BtnOpenLog_Click(object sender, RoutedEventArgs e) { new LogWindow().Show(); }

        private string GetTypeName(string id) => id.Contains('[') ? id.Substring(0, id.IndexOf('[')) : id;

        private void TxtSearchType_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_uniqueTypes != null)
                LstTypes.ItemsSource = _uniqueTypes.Where(t => t.ToLower().Contains(TxtSearchType.Text.ToLower())).ToList();
        }

        private void LstTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LstTypes.SelectedItem is string t)
            {
                LstInstances.ItemsSource = _allNodesCache
                    .Where(n => GetTypeName(n.Id) == t)
                    // Сортируем сначала по номеру, а затем по тексту (на всякий случай)
                    .OrderBy(n => GetSortIndex(n.Id))
                    .ThenBy(n => n.Id)
                    .ToList();
            }
        }

        private int GetSortIndex(string id)
        {
            // Пытаемся найти число внутри квадратных скобок [123]
            int start = id.IndexOf('[') + 1;
            int end = id.IndexOf(']');

            if (start > 0 && end > start)
            {
                string numPart = id.Substring(start, end - start);
                // Если там есть двоеточие (например [1:5]), берем только первую часть
                if (numPart.Contains(":")) numPart = numPart.Split(':')[0];

                if (int.TryParse(numPart, out int result))
                    return result;
            }
            return int.MaxValue; // Если числа нет, кидаем в конец списка
        }

        protected override void OnClosed(EventArgs e)
        {
            try { _excelSession.Save(); } catch { } // Автосейв при выходе
            _excelSession.Dispose();
            base.OnClosed(e);
        }
    }
}