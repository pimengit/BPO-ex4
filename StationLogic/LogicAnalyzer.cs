using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace BPO_ex4.StationLogic
{
    public class GroupInfo
    {
        public string OperatorType; // "OR", "AND", "V", "Unknown"
        public bool IsInverted;     // Стоит ли "!" перед группой
    }

    public static class LogicAnalyzer
    {
        private static Dictionary<string, string> _fileCache = new Dictionary<string, string>();

        // Папка, где лежат .cs файлы (рядом с exe)
        public static string LogicFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogicSheets");

        public static void LoadAllFiles()
        {
            if (!Directory.Exists(LogicFolderPath)) return;
            foreach (var file in Directory.GetFiles(LogicFolderPath, "*.cs"))
            {
                var name = Path.GetFileNameWithoutExtension(file);
                // Читаем весь файл, убираем переносы строк для простоты поиска
                var content = File.ReadAllText(file).Replace("\r", "").Replace("\n", " ");
                _fileCache[name] = content;
            }
        }

        public static GroupInfo AnalyzeGroup(string sheetName, int groupIndex)
        {
            if (!_fileCache.TryGetValue(sheetName, out string content))
                return new GroupInfo { OperatorType = "UNK", IsInverted = false };

            // Ищем паттерны:
            // !OR(5)  -> Type=OR, Inv=true
            // AND(5)  -> Type=AND, Inv=false
            // !V(2)   -> Type=V, Inv=true

            // Regex ищет: (знак ! возможен) (имя оператора) (пробелы) ( (индекс) )
            string pattern = $@"(!?)\s*(OR|AND|V)\s*\(\s*{groupIndex}\s*\)";

            var match = Regex.Match(content, pattern);
            if (match.Success)
            {
                return new GroupInfo
                {
                    IsInverted = match.Groups[1].Value == "!",
                    OperatorType = match.Groups[2].Value
                };
            }

            return new GroupInfo { OperatorType = "UNK", IsInverted = false };
        }
    }
}