using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BPO_ex4.StationLogic
{
    public static class CadDumpGenerator
    {
        // Модель для сериализации одной переменной
        private class NodeDumpModel
        {
            public string Id { get; set; }
            public string Description { get; set; }
            public string ClassName { get; set; }         // <- Имя листа Excel / Имя класса C#
            public List<string> Dependencies { get; set; }
            public string TemplateFormula { get; set; }   // <- Формула с "!"
            public string LogicRaw { get; set; }          // Разрешенные переменные
            public double OnDelayMs { get; set; }
            public double OffDelayMs { get; set; }
        }

        public static void GenerateJsonDump(Context ctx, string outputPath)
        {
            var allNodes = ctx.GetAllNodes().ToList();
            var groupedNodes = new Dictionary<string, List<NodeDumpModel>>();

            foreach (var node in allNodes)
            {
                // Для группировки в JSON берем всё до первого '_' или '[' (Например: SPEED)
                string groupName = GetGroupName(node.Id);

                if (!groupedNodes.ContainsKey(groupName))
                    groupedNodes[groupName] = new List<NodeDumpModel>();

                // !!! ИСПРАВЛЕНИЕ !!!
                // Получаем имя класса, отрезая только скобки. 
                // "SPEED_70U[15]" -> "SPEED_70U"
                string className = GetSheetName(node.Id);

                // Достаем формулу с инверсиями из твоего парсера файлов, используя правильное имя!
                string template = LogicAnalyzer.GetRawFormula(className);

                groupedNodes[groupName].Add(new NodeDumpModel
                {
                    Id = node.Id,
                    Description = node.Description?.Trim() ?? "",
                    ClassName = className,
                    Dependencies = GetDependencies(node),
                    TemplateFormula = template,

                    // ВЫЗЫВАЕМ НОВЫЙ МЕТОД: передаем и ноду, и её шаблон
                    LogicRaw = GetResolvedLogicString(node, template),

                    OnDelayMs = node.OnDelay.TotalMilliseconds,
                    OffDelayMs = node.OffDelay.TotalMilliseconds
                });
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            string json = JsonSerializer.Serialize(groupedNodes, options);
            File.WriteAllText(outputPath, json);
        }

        // Старый GetLogicString оставляем без изменений
        // Замени старый GetLogicString на этот:
        private static string GetResolvedLogicString(Node node, string templateFormula)
        {
            // Если шаблона нет (например, для простых констант), лепим старым способом
            if (string.IsNullOrWhiteSpace(templateFormula))
            {
                if (node.LogicSource?.Groups == null || !node.LogicSource.Groups.Any()) return "";
                var orParts = new List<string>();
                foreach (var group in node.LogicSource.Groups)
                {
                    if (group == null || !group.Any()) continue;
                    orParts.Add("(" + string.Join(" && ", group.Select(n => n.Id)) + ")");
                }
                return string.Join(" || ", orParts);
            }

            // Регулярка ищет паттерны типа V(1), AND(2), OR(5)
            return Regex.Replace(templateFormula, @"(?:V|AND|OR|v|and|or)\s*\(\s*(\d+)\s*\)", match =>
            {
                if (int.TryParse(match.Groups[1].Value, out int idx))
                {
                    // !!! ИСПРАВЛЕНИЕ: Добавляем .ToList(), чтобы гарантировать 
                    // наличие свойства .Count и возможность обращения по индексу [arrayIdx]
                    var groups = node.LogicSource?.Groups?.ToList();

                    if (groups != null)
                    {
                        // В массивах C# индексация с 0, поэтому V(1) лежит под индексом 0
                        int arrayIdx = idx - 1;

                        // Защита от выхода за пределы
                        if (arrayIdx >= 0 && arrayIdx < groups.Count)
                        {
                            var group = groups[arrayIdx];
                            if (group != null && group.Any())
                            {
                                // Подставляем реальное имя переменной (например, SECT_P[18])
                                // Оборачиваем в скобки, если аргументов несколько, чтобы логика не сломалась
                                var parts = group.Select(n => n.Id).ToList();
                                if (parts.Count > 1)
                                    return "(" + string.Join(" && ", parts) + ")";
                                else
                                    return parts.FirstOrDefault() ?? "";
                            }
                        }
                    }
                }
                return match.Value; // Если не нашли переменную, оставляем как есть (например "V(1)")
            });
        }

        // --- Хелперы ---

        // Для поиска формулы в кэше .cs файлов: отрезаем только [индекс]
        private static string GetSheetName(string id)
        {
            int bracketIdx = id.IndexOf('[');
            if (bracketIdx > 0)
                return id.Substring(0, bracketIdx);
            return id; // Если скобок нет, возвращаем как есть (например CONST_1)
        }

        // Для группировки в JSON: отрезаем всё после _ или [
        private static string GetGroupName(string id)
        {
            int underscoreIdx = id.IndexOf('_');
            int bracketIdx = id.IndexOf('[');

            if (underscoreIdx > 0 && bracketIdx > 0)
                return id.Substring(0, Math.Min(underscoreIdx, bracketIdx));

            if (underscoreIdx > 0) return id.Substring(0, underscoreIdx);
            if (bracketIdx > 0) return id.Substring(0, bracketIdx);

            return id;
        }

        private static List<string> GetDependencies(Node node)
        {
            var deps = new HashSet<string>();
            if (node.LogicSource?.Groups != null)
            {
                foreach (var group in node.LogicSource.Groups)
                {
                    if (group == null) continue;
                    foreach (var parent in group)
                    {
                        deps.Add(parent.Id);
                    }
                }
            }
            return deps.ToList();
        }
    }
}