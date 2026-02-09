using System.Windows;

namespace BPO_ex4
{
    public partial class LogWindow : Window
    {
        public LogWindow()
        {
            InitializeComponent();
            // Привязываем список к нашему статическому логгеру
            LstLog.ItemsSource = AppLogger.Messages;
        }
    }
}