using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BPO_ex4.StationLogic
{
    public static class DeepAnalyzer
    {
        public static List<string> Analyze(Context ctx)
        {
            var report = new List<string>();
            var allNodes = ctx.GetAllNodes().ToList();

            // 1. БЭКАП СОСТОЯНИЯ (Сохраняем текущие значения, чтобы не сломать UI)
            var backup = new Dictionary<Node, bool>();
            foreach (var node in allNodes) backup[node] = node.Value;

            // 2. "ВЫДЕРГИВАЕМ ШНУР" (Сброс в 0)
            // Оставляем True только у источников питания (Константы, Входы)
            foreach (var node in allNodes)
            {
                if (IsPowerSource(node.Id))
                {
                    node.Value = true; // Это дает ток
                }
                else
                {
                    node.Value = false; // Все остальное обесточено
                }
            }

            // 3. СИМУЛЯЦИЯ РАЗГОНА (Пытаемся "зажечь" схему)
            bool changed = true;
            int passes = 0;

            // Крутим цикл, пока появляются новые единицы
            while (changed)
            {
                changed = false;
                passes++;

                if (passes > 5000) break; // Защита

                foreach (var node in allNodes)
                {
                    // Если уже горит - пропускаем
                    if (node.Value) continue;

                    // Если метода Compute нет - пропускаем
                    if (node.Compute == null) continue;

                    // !!! САМОЕ ГЛАВНОЕ !!!
                    // Мы выполняем ТВОЙ C# КОД.
                    // Если там написано return V(1) && !V(2)... оно выполнится с текущими значениями.
                    // Если V зависит сама от себя и сейчас 0 -> она вернет 0.

                    bool result = false;
                    try
                    {
                        result = node.Compute();
                    }
                    catch
                    {
                        // Если формула упала (например, индекс кривой), считаем 0
                        result = false;
                    }

                    if (result)
                    {
                        node.Value = true; // ОЖИЛА!
                        changed = true;    // Запускаем новый круг, вдруг она кого-то еще зажжет
                    }
                }
            }

            // 4. СБОР ТРУПОВ
            // Те, кто остался false, хотя имеет формулу Compute — мертвы.
            foreach (var node in allNodes)
            {
                // Проверяем, есть ли у ноды вообще логика (Compute не заглушка)
                // Обычно заглушка возвращает false, но у нас и value false. 
                // Можно проверить node.LogicSource != null как признак "это не воздух"

                bool isLogicNode = node.LogicSource != null && node.Compute != null;

                // Исключаем выходы, которые и должны быть 0 (если логика так решила), 
                // но нас интересуют "структурно мертвые".
                // В данном методе мы не отличим "Просто выключено" от "Невозможно включить".
                // НО! Так как мы включили ВСЕ ВХОДЫ (IsPowerSource -> True),
                // то если переменная не включилась даже при всех нажатых кнопках — она мертва.

                if (isLogicNode && !node.Value)
                {
                    // Пытаемся объяснить причину (анализ первого уровня)
                    string reason = TryExplain(node, ctx);
                    report.Add($"[DEAD] {node.Id}\n      Возможная причина: {reason}");
                }
            }

            // 5. ВОССТАНОВЛЕНИЕ СОСТОЯНИЯ
            foreach (var kvp in backup)
            {
                kvp.Key.Value = kvp.Value;
            }

            report.Add($"\nСимуляция завершена за {passes} проходов.");
            return report;
        }

        // --- ПОПЫТКА ОБЪЯСНИТЬ ПРИЧИНУ (Без парсинга C#) ---
        private static string TryExplain(Node node, Context ctx)
        {
            // Мы не можем залезть внутрь Compute(), но мы можем посмотреть на Groups,
            // если они у тебя заполнены из Excel. Они обычно показывают зависимости.

            if (node.LogicSource?.Groups == null) return "Формула вернула FALSE (при всех активных входах).";

            // Ищем родителей, которые тоже False
            var deadParents = new List<string>();
            foreach (var group in node.LogicSource.Groups)
            {
                if (group == null) continue;
                foreach (var parent in group)
                {
                    if (parent.Value == false) // Значит этот родитель тоже мертв
                    {
                        if (parent == node) deadParents.Add("САМА СЕБЯ (Самоблокировка)");
                        else deadParents.Add(parent.Id);
                    }
                }
            }

            if (deadParents.Count > 0)
                return "Зависит от мертвых переменных: " + string.Join(", ", deadParents.Distinct().Take(3));

            return "Сложная логика (инверсия?) не пускает сигнал.";
        }

        private static bool IsPowerSource(string id)
        {
            var u = id.ToUpper();
            // Включаем ВСЁ, что может управлять схемой
            if (u == "1" || u == "TRUE" || u == "CONST_1" || u == "P" || u == "ПЛЮС") return true;

            if (u.Contains("ROUTE_KN")) return true;
            if (u.StartsWith("INPUT_")) return true;
            if (u.Contains("SENS")) return true;
            if (u.StartsWith("BTN_")) return true;
            if (u.StartsWith("SW_")) return true;
            if (u.StartsWith("ВХОД")) return true;
            if (u.StartsWith("DI_")) return true;

            return false;
        }
    }
}