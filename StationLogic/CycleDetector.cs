using System;
using System.Collections.Generic;
using System.Linq;

namespace BPO_ex4.StationLogic
{
    // Результат анализа топологии (циклы и фантомы)
    public class TopologyResult
    {
        public List<string> AlgebraicLoops { get; set; } = new List<string>();
        public List<string> UndefinedInputs { get; set; } = new List<string>();
    }

    public static class CycleDetector
    {
        public static TopologyResult Analyze(Context ctx)
        {
            var result = new TopologyResult();
            var allNodes = ctx.GetAllNodes().ToList();

            // 1. ПРОВЕРКА НА ЗАЦИКЛИВАНИЕ (DFS)
            var visited = new HashSet<Node>();
            var recursionStack = new HashSet<Node>();

            foreach (var node in allNodes)
            {
                // Если мы нашли цикл, добавляем его в отчет
                if (CheckCycle(node, visited, recursionStack, out string cyclePath))
                {
                    // Чтобы не дублировать один и тот же цикл много раз, можно добавить проверку
                    // Но пока просто пишем
                    result.AlgebraicLoops.Add(cyclePath);
                }
            }

            // 2. ПОИСК ПЕРЕМЕННЫХ БЕЗ ЛОГИКИ (которые не являются входами)
            foreach (var node in allNodes)
            {
                // Есть ли у ноды логика?
                bool hasLogic = node.LogicSource != null &&
                                node.LogicSource.Groups != null &&
                                node.LogicSource.Groups.Any();

                // Является ли она внешним входом (Кнопка, Датчик)?
                bool isInput = IsExternalInput(node.Id) || IsConstant(node.Id);

                if (!hasLogic && !isInput)
                {
                    result.UndefinedInputs.Add($"{node.Id} ({node.Description})");
                }
            }

            // Удаляем дубликаты циклов (простая фильтрация)
            result.AlgebraicLoops = result.AlgebraicLoops.Distinct().ToList();

            return result;
        }

        // --- Алгоритм поиска цикла (Depth First Search) ---
        private static bool CheckCycle(Node node, HashSet<Node> visited, HashSet<Node> recursionStack, out string path)
        {
            path = "";

            // ВАЖНО: Если у ноды есть задержка (таймер), она РАЗРЫВАЕТ алгебраическую петлю.
            if (node.OnDelay.TotalMilliseconds > 0 || node.OffDelay.TotalMilliseconds > 0)
            {
                return false;
            }

            // Если нода уже в текущем стеке рекурсии -> ЦИКЛ НАЙДЕН
            if (recursionStack.Contains(node))
            {
                path = $"{node.Id}";
                return true;
            }

            // Если мы эту ноду уже проверяли раньше и вышли из неё -> цикла тут нет
            if (visited.Contains(node))
            {
                return false;
            }

            visited.Add(node);
            recursionStack.Add(node);

            // Рекурсивно проверяем родителей
            if (node.LogicSource?.Groups != null)
            {
                foreach (var group in node.LogicSource.Groups)
                {
                    if (group == null) continue;
                    foreach (var parent in group)
                    {
                        if (CheckCycle(parent, visited, recursionStack, out string subPath))
                        {
                            path = $"{node.Id} -> {subPath}";
                            // Убираем из стека перед выходом, чтобы не ломать другие ветки
                            recursionStack.Remove(node);
                            return true;
                        }
                    }
                }
            }

            recursionStack.Remove(node);
            return false;
        }

        private static bool IsExternalInput(string id)
        {
            // Настрой фильтр под свои имена
            return id.Contains("ROUTE_KN") ||
                   id.StartsWith("INPUT_") ||
                   id.Contains("SENS") ||
                   id.StartsWith("BTN_") ||
                   id.Contains("Вход");
        }

        private static bool IsConstant(string id)
        {
            var u = id.ToUpper();
            return u == "1" || u == "TRUE" || u == "CONST_1" || u == "P" || u == "0" || u == "FALSE" || u == "M";
        }
    }
}