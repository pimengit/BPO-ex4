using BPO_ex4.StationLogic;
using System.Windows;
using System.Windows.Controls;

namespace BPO_ex4
{
    public partial class MainWindow : Window
    {
        private GlobalStationLinker _linker = new GlobalStationLinker();
        private int _stationCounter = 1;

        public MainWindow()
        {
            OfficeOpenXml.ExcelPackage.License.SetNonCommercialPersonal("User");
            LogicAnalyzer.LoadAllFiles();
            InitializeComponent();

            // Создаем первую станцию по умолчанию при запуске
            AddNewStationTab();
        }

        // Клик по плюсику
        private void AddStation_Click(object sender, RoutedEventArgs e)
        {
            AddNewStationTab();
        }

        private void AddNewStationTab()
        {
            string name = $"Станция {_stationCounter++}";

            // Создаем наш независимый контрол-редактор
            var editorControl = new StationEditorControl();
            editorControl.StationName = name;

            // Создаем вкладку для него
            var tab = new TabItem
            {
                Header = name,
                Content = editorControl // Засовываем редактор внутрь вкладки
            };

            StationsTabs.Items.Add(tab);
            StationsTabs.SelectedItem = tab;
        }

        // Вызов Линкера
        private void OpenLinker_Click(object sender, RoutedEventArgs e)
        {
            _linker.ClearStations();
            var currentStations = new List<StationInstance>();

            // Собираем все станции из открытых вкладок
            foreach (TabItem tab in StationsTabs.Items)
            {
                if (tab.Content is StationEditorControl editor && editor.StationCtx != null)
                {
                    // Собираем данные вкладки в коробку StationInstance
                    var instance = new StationInstance(editor.StationName)
                    {
                        Ctx = editor.StationCtx,
                        Engine = editor.StationEngine
                    };

                    _linker.AddStation(instance);
                    currentStations.Add(instance);
                }
            }

            if (currentStations.Count < 2)
            {
                MessageBox.Show("Для настройки увязки нужно создать хотя бы 2 станции (используйте кнопку +)!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Открываем наше новое окно, передавая ему список станций
            var linkerWindow = new LinkerWindow(currentStations, _linker);
            linkerWindow.Owner = this; // Чтобы оно красиво открылось по центру главного окна
            linkerWindow.ShowDialog(); // ShowDialog блокирует главное окно, пока не настроишь порты
        }
    }
}