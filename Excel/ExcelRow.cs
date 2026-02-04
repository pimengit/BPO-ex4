namespace BPO_ex4.Excel
{
    public class ExcelRow
    {
        public int ObjectIndex { get; set; }

        // Источник — КОД ("2.3", "3.1", "0.0")
        public string[] Ist { get; set; }

        // № — может отсутствовать
        public int?[] Num { get; set; }
    }
}
