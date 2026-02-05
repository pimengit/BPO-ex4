using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace BPO_ex4.StationLogic
{
    public class TableRow
    {
        public int RowIndex { get; set; }
        public Dictionary<int, string> Cells { get; set; } = new Dictionary<int, string>();
    }

    public static class LogicAnalyzer
    {
        private static Dictionary<string, string> _fileCache = new Dictionary<string, string>();
        public static string LogicFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogicSheets");

        public static void LoadAllFiles()
        {
            _fileCache.Clear();
            if (!Directory.Exists(LogicFolderPath)) return;

            foreach (var file in Directory.GetFiles(LogicFolderPath, "*.cs"))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                string content = File.ReadAllText(file);

                // Очистка от мусора
                content = Regex.Replace(content, @"//.*", "");
                content = content.Replace("\r", " ").Replace("\n", " ").Replace("\t", " ");

                // Мягкий поиск return
                var match = Regex.Match(content, @"return\s*([\s\S]*?);");
                if (match.Success)
                {
                    _fileCache[name] = match.Groups[1].Value.Trim();
                }
            }
        }

        public static List<TableRow> GetTruthTable(string sheetName)
        {
            var rows = new List<TableRow>();

            // Если файла нет в кэше, создаем "Фейковую" строку
            // Это нужно, чтобы в UI отрисовались хотя бы ЗАГОЛОВКИ столбцов
            if (!_fileCache.TryGetValue(sheetName, out string code))
            {
                // Fallback: одна пустая строка
                rows.Add(new TableRow { RowIndex = 0 });
                return rows;
            }

            // Парсим реальный код
            var parts = code.Split(new[] { "||" }, StringSplitOptions.None);
            for (int i = 0; i < parts.Length; i++)
            {
                var rawRow = parts[i];
                var tableRow = new TableRow { RowIndex = i };
                var matches = Regex.Matches(rawRow, @"(!?)\s*(?:V|OR|AND)\s*\(\s*(\d+)\s*\)");

                foreach (Match m in matches)
                {
                    bool isNot = m.Groups[1].Value == "!";
                    if (int.TryParse(m.Groups[2].Value, out int groupIdx))
                    {
                        tableRow.Cells[groupIdx] = isNot ? "0" : "1";
                    }
                }
                rows.Add(tableRow);
            }
            return rows;
        }

        public static string GetGroupType(string sheetName, int groupIdx)
        {
            if (!_fileCache.TryGetValue(sheetName, out string code)) return "";
            var m = Regex.Match(code, $@"(OR|AND|V)\s*\(\s*{groupIdx}\s*\)");
            return m.Success ? m.Groups[1].Value : "";
        }
    }
}