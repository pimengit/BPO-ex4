using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BPO_ex4.StationLogic;
using BPO_ex4.Excel;

namespace BPO_ex4
{
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
        private ExcelSession _session;
        private List<Node> _allNodes;

        public EditNodeWindow(Node target, int groupIdx, int inputIdx, ExcelSession session, List<Node> allNodes)
        {
            InitializeComponent();
            _targetNode = target;
            _groupIndex = groupIdx;
            _inputIndex = inputIdx;
            _session = session;
            _allNodes = allNodes;

            LoadData();
            UpdateUi();
        }

        private void LoadData()
        {
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
                CmbIndex.ItemsSource = null;
            }
        }

        private void CmbVariable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbVariable.SelectedItem is SourceMeta meta)
            {
                var existingInstances = _allNodes
                    .Where(n => GetTypeName(n.Id) == meta.TypeName)
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

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CmbVariable.SelectedItem is not SourceMeta selectedMeta)
                {
                    MessageBox.Show("Select variable type!");
                    return;
                }

                if (!int.TryParse(CmbIndex.Text, out int indexNum))
                {
                    MessageBox.Show("Enter valid index!");
                    return;
                }

                string newId = $"{selectedMeta.TypeName}[{indexNum}]";
                var sourceNode = new Node { Id = newId, Value = false };

                string sheetName = GetTypeName(_targetNode.Id);
                int objectIndex = GetIndex(_targetNode.Id);

                // ВЫЗОВ МЕТОДОВ SESSION (ТЕПЕРЬ СИГНАТУРЫ СОВПАДАЮТ)
                // Внутри BtnSave_Click, перед закрытием окна:

                // ... создание sourceNode ...

                // 1. Пишем в Excel (Сессия)
                if (_inputIndex == -1)
                    _session.AddInputInMemory(sheetName, objectIndex, _groupIndex, sourceNode);
                else
                    _session.UpdateCellInMemory(sheetName, objectIndex, _groupIndex, _inputIndex, sourceNode);

                // 2. ПИШЕМ В ЛОГИКУ (ПАМЯТЬ) - ЧТОБЫ БЫЛО МГНОВЕННО!
                if (_inputIndex == -1)
                    LogicPatcher.AddInputToRuntime(_targetNode, _groupIndex, sourceNode);
                else
                    LogicPatcher.UpdateInputInRuntime(_targetNode, _groupIndex, _inputIndex, sourceNode);

                // 3. Сохраняем файл на диск (можно убрать, если хочешь сохранять только при выходе)
                _session.Save();

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving: {ex.Message}");
            }
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