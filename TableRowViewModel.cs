using System.Collections.ObjectModel;

namespace BPO_ex4.ViewModels
{
    public class TableCell
    {
        public string Text { get; set; }
        public bool IsEmpty => string.IsNullOrEmpty(Text);
        public bool IsInverted => Text == "0";
        public bool IsActive { get; set; }

        // НОВОЕ ПОЛЕ ДЛЯ ОТОБРАЖЕНИЯ ТАЙМЕРА
        public string TimerInfo { get; set; }
    }

    public class TableRowViewModel
    {
        public int RowId { get; set; }
        public ObservableCollection<TableCell> Cells { get; set; } = new ObservableCollection<TableCell>();
        public bool IsRowActive { get; set; }
    }

    // Вспомогательные классы для заголовков
    public class ColumnHeader
    {
        public int GroupIndex { get; set; }
        public string Title { get; set; }
        public string OperatorType { get; set; }
        public bool CanAddInput => OperatorType == "OR" || OperatorType == "AND";
        public System.Collections.Generic.List<NodeWrapper> Nodes { get; set; }
    }

    public class NodeWrapper
    {
        public StationLogic.Node LogicNode { get; set; }
        public string Id => LogicNode.Id;
        public bool Value => LogicNode.Value;
        public string Description => LogicNode.Description;
    }
}