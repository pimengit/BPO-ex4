using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace BPO_ex4
{
    public partial class LogWindow : Window
    {
        private ICollectionView _logView;

        public LogWindow()
        {
            InitializeComponent();
            _logView = CollectionViewSource.GetDefaultView(AppLogger.Messages);
            _logView.Filter = LogFilter;
            LstLog.ItemsSource = _logView;
        }

        private bool LogFilter(object item)
        {
            if (string.IsNullOrEmpty(TxtFilter.Text)) return true;
            string logEntry = item as string;
            if (string.IsNullOrEmpty(logEntry)) return false;
            return logEntry.IndexOf(TxtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void TxtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            _logView.Refresh();
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            TxtFilter.Text = string.Empty;
            TxtFilter.Focus();
        }
    }

    // !!! ВОТ ЭТОТ КЛАСС НУЖНО ДОБАВИТЬ !!!
    public class StringContainsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s && parameter is string p)
            {
                return s.Contains(p);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}