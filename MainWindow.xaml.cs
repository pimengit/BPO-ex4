using BPO_ex4.Excel;       // Проверь namespace
using BPO_ex4.StationLogic; // Проверь namespace
using Microsoft.Win32;     // Для OpenFileDialog
using OfficeOpenXml;
using System.Windows;

namespace BPO_ex4
{
    public partial class MainWindow : Window
    {
        private Context _ctx;

        public MainWindow()
        {
            ExcelPackage.License.SetNonCommercialPersonal("<Your Name>");
            InitializeComponent();
            _ctx = new Context();
            // Инициализация констант
            _ctx.Set("CONST_0", false);
            _ctx.Set("CONST_1", true);
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "Excel Files|*.xlsx";

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    // Загружаем логику (старый код из Program.cs)
                    ExcelParser.Load(dlg.FileName, _ctx);
                    _ctx.RecomputeAll();

                    TxtStatus.Text = $"Loaded! Nodes count: {_ctx.GetAllNodesCount()}"; // Нужно добавить метод GetAllNodesCount в Context или просто написать "Success"
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }
        }
    }
}