using System.Collections.Generic;
using System.Collections.ObjectModel;
using BPO_ex4.StationLogic;

namespace BPO_ex4.ViewModels
{
    // Ячейка таблицы (пересечение строки и столбца)
    public class TableCell
    {
        public string Text { get; set; } // "1" или "0"
        public bool IsEmpty => string.IsNullOrEmpty(Text);
        public bool IsInverted => Text == "0"; // Для визуализации "!"
        public bool IsActive { get; set; } // Выполняется ли условие
    }

    // Строка таблицы
    public class TableRowViewModel
    {
        public int RowId { get; set; }
        public ObservableCollection<TableCell> Cells { get; set; } = new ObservableCollection<TableCell>();
        public bool IsRowActive { get; set; }
    }

    // Заголовок столбца
    public class ColumnHeader
    {
        public int GroupIndex { get; set; }
        public string Title { get; set; } // Имя (напр. "MAR")
        public string OperatorType { get; set; } // OR, AND, V
        public bool CanAddInput => OperatorType == "OR" || OperatorType == "AND"; // Условие для кнопки "+"

        public List<NodeWrapper> Nodes { get; set; } // Список кнопок-переменных
    }

    // Обертка для переменной в заголовке
    public class NodeWrapper
    {
        public Node LogicNode { get; set; }
        public string Id => LogicNode.Id;
        public bool Value => LogicNode.Value;
        public string Description => LogicNode.Description;
    }
}