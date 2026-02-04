using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BPO_ex4.StationLogic; // Для ReverseSourceRules и Node
using BPO_ex4.Excel;        // Для ExcelUpdater

namespace BPO_ex4
{
    public partial class EditNodeWindow : Window
    {
        private Node _targetNode;
        private int _groupIndex;
        private int _inputIndex; // -1 означает "Добавление нового", >=0 означает "Редактирование"
        private string _excelPath;

        /// <summary>
        /// Конструктор окна редактирования/добавления.
        /// </summary>
        /// <param name="target">Целевая нода (куда подключаем)</param>
        /// <param name="groupIdx">Номер группы логики (AND/OR)</param>
        /// <param name="inputIdx">Индекс входа в группе. Передай -1 для ДОБАВЛЕНИЯ.</param>
        /// <param name="excelPath">Путь к файлу Excel</param>
        public EditNodeWindow(Node target, int groupIdx, int inputIdx, string excelPath)
        {
            InitializeComponent();
            _targetNode = target;
            _groupIndex = groupIdx;
            _inputIndex = inputIdx;
            _excelPath = excelPath;

            LoadData();
            UpdateUi();
        }

        private void LoadData()
        {
            // Загружаем категории из ReverseSourceRules
            CmbCategory.ItemsSource = ReverseSourceRules.GetCategories();
        }

        private void UpdateUi()
        {
            LblTarget.Text = $"{_targetNode.Id} ({_targetNode.Description})";
            TxtGroupInfo.Text = _groupIndex.ToString();

            if (_inputIndex == -1)
            {
                TxtTitle.Text = "Add New Connection";
                TxtModeInfo.Text = "[New Input]";
            }
            else
            {
                TxtTitle.Text = "Edit Existing Connection";
                TxtModeInfo.Text = $"[Input #{_inputIndex + 1}]";
            }
        }

        private void CmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbCategory.SelectedItem is string category)
            {
                CmbVariable.ItemsSource = ReverseSourceRules.GetVariables(category);
                CmbVariable.SelectedIndex = -1;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Валидация
                if (CmbVariable.SelectedItem is not SourceMeta selectedMeta)
                {
                    MessageBox.Show("Please select a variable type.");
                    return;
                }

                if (!int.TryParse(TxtIndex.Text, out int indexNum))
                {
                    MessageBox.Show("Index must be a valid number.");
                    return;
                }

                // 2. Создаем временную ноду-источник
                // (Updater'у нужна нода, чтобы получить из нее ID и распарсить обратно в Excel код)
                string newId = $"{selectedMeta.TypeName}[{indexNum}]";
                var sourceNode = new Node { Id = newId, Value = false }; // Value не важно для записи

                // 3. Сохраняем в Excel
                // Нам нужно распарсить ID целевой ноды, чтобы узнать имя листа и индекс объекта
                // Пример ID: "AVTODO_AR[8]" -> Sheet="AVTODO_AR", Index=8
                string sheetName = GetSheetName(_targetNode.Id);
                int objectIndex = GetObjectIndex(_targetNode.Id);

                if (_inputIndex == -1)
                {
                    // РЕЖИМ ДОБАВЛЕНИЯ (ADD)
                    ExcelUpdater.AddInputToGroup(_excelPath, sheetName, objectIndex, _groupIndex, sourceNode);
                }
                else
                {
                    // РЕЖИМ РЕДАКТИРОВАНИЯ (UPDATE)
                    ExcelUpdater.UpdateCell(_excelPath, sheetName, objectIndex, _groupIndex, _inputIndex, sourceNode);
                }

                MessageBox.Show("Successfully saved to Excel!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving to Excel:\n{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Хелперы для парсинга ID (можно вынести в общий класс)
        private string GetSheetName(string id)
        {
            int idx = id.IndexOf('[');
            return idx > 0 ? id.Substring(0, idx) : id;
        }

        private int GetObjectIndex(string id)
        {
            int start = id.IndexOf('[') + 1;
            int end = id.IndexOf(']');
            if (start > 0 && end > start)
            {
                if (int.TryParse(id.Substring(start, end - start), out int result))
                    return result;
            }
            return 0; // Ошибка или константа
        }
    }
}