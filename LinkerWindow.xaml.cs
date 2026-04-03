using System.Collections.Generic;
using System.Windows;

namespace BPO_ex4
{
    public partial class LinkerWindow : Window
    {
        private GlobalStationLinker _linker;

        // В конструктор передаем список доступных станций и ссылку на глобальный линкер
        public LinkerWindow(List<StationInstance> availableStations, GlobalStationLinker linker)
        {
            InitializeComponent();
            _linker = linker;

            // Привязываем списки станций к ComboBox'ам
            CbStationA.ItemsSource = availableStations;
            CbStationB.ItemsSource = availableStations;

            // Выбираем первые элементы по умолчанию (чтобы не было пустоты)
            if (availableStations.Count > 0)
            {
                CbStationA.SelectedIndex = 0;
                CbStationB.SelectedIndex = availableStations.Count > 1 ? 1 : 0;
            }

            CbPortA.SelectedIndex = 0;
            CbPortB.SelectedIndex = 0;
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            var stA = CbStationA.SelectedItem as StationInstance;
            var stB = CbStationB.SelectedItem as StationInstance;

            if (stA == null || stB == null)
            {
                MessageBox.Show("Сначала загрузите и выберите станции!", "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (stA.StationName == stB.StationName)
            {
                MessageBox.Show("Вы пытаетесь соединить станцию саму с собой. Выберите разные станции!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Индексы в ComboBox начинаются с 0, поэтому прибавляем 1, чтобы получить номер порта (1-4)
            int portA = CbPortA.SelectedIndex + 1;
            int portB = CbPortB.SelectedIndex + 1;

            // Вызываем магию увязки из класса GlobalStationLinker
            _linker.ConnectPorts(stA.StationName, portA, stB.StationName, portB);

            MessageBox.Show($"Успешно!\n\nДанные из {stA.StationName} (Порт {portA}) теперь передаются в {stB.StationName} (Порт {portB}).", "Увязка установлена", MessageBoxButton.OK, MessageBoxImage.Information);

            // Закрываем окно после успешного соединения
            this.Close();
        }
    }
}