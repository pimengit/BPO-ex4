using System.Collections.Generic;

namespace BPO_ex4.Excel
{
    public class ExcelSheet
    {
        public string Name;          // имя вкладки
        public int ColumnCount;      // сколько колонок
        public List<ExcelRow> Rows;  // строки = объекты
    }
}
