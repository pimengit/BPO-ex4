using BPO_ex4.Excel;
using BPO_ex4.StationLogic;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

            // Проверяем дубликаты (опционально)
            foreach (TabItem item in MainTabs.Items)
            {
                if ((item.Tag as LogicTabContent)?.Name == node.Id)
                {
                    MainTabs.SelectedItem = item;
                    //return; // Раскомментируй, если не хочешь открывать одну переменную дважды
                }
            }

            // 1. Создаем контент вкладки
            var content = new LogicTabContent();
            // Передаем ей "мозги" приложения
            content.Initialize(_ctx, _engine, _excelSession, _allNodesCache, node);

            // 2. Подписываемся на события от вкладки
            content.NewTabRequested += OpenNewTab;      // Вкладка хочет открыть другую вкладку
            content.GlobalChangeRequested += OnGlobalChange; // Вкладка что-то изменила

            // 3. Создаем саму вкладку
            var tab = new TabItem
            {
                Header = node.Id,
                Content = content,
                Tag = content
            };

            MainTabs.Items.Add(tab);
            MainTabs.SelectedItem = tab;
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
                LstInstances.ItemsSource = _allNodesCache.Where(n => GetTypeName(n.Id) == t).OrderBy(n => n.Id).ToList();
        }

        protected override void OnClosed(EventArgs e)
        {
            try { _excelSession.Save(); } catch { } // Автосейв при выходе
            _excelSession.Dispose();
            base.OnClosed(e);
        }
    }
}