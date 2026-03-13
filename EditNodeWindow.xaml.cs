using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BPO_ex4.StationLogic;
using BPO_ex4.Excel; // Возвращаем твой неймспейс для ReverseSourceRules

namespace BPO_ex4
{
    // Класс для выпадающего списка индексов
    public class InstanceItem
    {
        public int Index { get; set; }
        public string Description { get; set; }
        public override string ToString() => Index.ToString();
    }

    public partial class EditNodeWindow : Window
    {
        private Node _targetNode;
        private int _groupIndex;
        private int _inputIndex;
        private List<Node> _allNodes;

        public Node ResultSourceNode { get; private set; }
        public string TargetSheetName { get; private set; }
        public int TargetObjectIndex { get; private set; }

        public EditNodeWindow(Node target, int groupIdx, int inputIdx, List<Node> allNodes)
        {
            InitializeComponent();
            _targetNode = target;
            _groupIndex = groupIdx;
            _inputIndex = inputIdx;
            _allNodes = allNodes;

            TargetSheetName = GetTypeName(target.Id);
            TargetObjectIndex = GetIndex(target.Id);

            LoadData();
            //UpdateUi();
        }

        private void LoadData()
        {
            // 1. Берем разрешенные переменные для текущей категории (целевого класса)
            var allowedMetas = ReverseSourceRules.GetVariables(GetCategoryName(TargetSheetName));

            var allowedTypes = new List<string>
            {
                "CONST_1",
                "CONST_0"
            };

            // 2. Добавляем имена разрешенных типов из правил
            if (allowedMetas != null)
            {
                allowedTypes.AddRange(allowedMetas.Select(m => m.TypeName));
            }

            // 3. Биндим к ComboBox
            CmbVariable.ItemsSource = allowedTypes;
        }

        /*private void UpdateUi()
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
        }*/

        private void CmbVariable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbVariable.SelectedItem is string selectedType)
            {
                // Если константа - индекс не нужен, блокируем поле
                if (selectedType == "CONST_1" || selectedType == "CONST_0")
                {
                    CmbIndex.ItemsSource = null;
                    CmbIndex.Text = "";
                    CmbIndex.IsEnabled = false;
                }
                else
                {
                    // Иначе разблокируем и ищем все существующие переменные этого типа на станции
                    CmbIndex.IsEnabled = true;
                    var existingInstances = _allNodes
                        .Where(n => GetTypeName(n.Id) == selectedType)
                        .Select(n => new InstanceItem
                        {
                            Index = GetIndex(n.Id),
                            Description = n.Description
                        })
                        .OrderBy(x => x.Index)
                        .ToList();

                    CmbIndex.ItemsSource = existingInstances;
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CmbVariable.SelectedItem is not string selectedType)
                {
                    MessageBox.Show("Select variable type!");
                    return;
                }

                string newId;

                if (selectedType == "CONST_1" || selectedType == "CONST_0")
                {
                    newId = selectedType;
                }
                else
                {
                    if (!int.TryParse(CmbIndex.Text, out int indexNum))
                    {
                        MessageBox.Show("Enter valid index!");
                        return;
                    }
                    newId = $"{selectedType}[{indexNum}]";
                }

                // Создаем результат
                ResultSourceNode = new Node { Id = newId, Value = false };

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }


        private string GetCategoryName(string id)
        {
            int idx = id.IndexOf('_');
            return idx > 0 ? id.Substring(0, idx) : id;
        }
        private string GetTypeName(string id)
        {
            int idx = id.IndexOf('[');
            return idx > 0 ? id.Substring(0, idx) : id;
        }

        private int GetIndex(string id)
        {
            int start = id.IndexOf('[') + 1;
            int end = id.IndexOf(']');
            if (start > 0 && end > start && int.TryParse(id.Substring(start, end - start), out int res))
                return res;
            return 0;
        }
    }
}