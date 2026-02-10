using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BPO_ex4.StationLogic
{
    public class Node
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public bool Value { get; set; }
        public SheetLogic LogicSource { get; set; }

        public List<Node> Dependents { get; set; } = new List<Node>();

        // Делегат для вычисления (подменяется в VariableFactory)
        public Func<bool> Compute { get; set; } = () => false;

        /*public override string ToString()
        {
            return $"{Id} = {Value}";
        }*/

        // Событие для логов и внешних подписчиков
        public event Action<Node>? Changed;

        // Событие: "Таймер прошел, я готова применить отложенное значение"
        public event Action<Node>? DelayedUpdateReady;


        public TimeSpan OnDelay { get; set; } = TimeSpan.Zero;
        public TimeSpan OffDelay { get; set; } = TimeSpan.Zero;

        // Внутреннее состояние для задержек
        private bool _pendingValue;
        private bool _hasPending;
        private object _lock = new object();

        /// <summary>
        /// Пересчитывает логику. 
        /// Возвращает TRUE, если значение изменилось МГНОВЕННО.
        /// Возвращает FALSE, если значение не изменилось ИЛИ запустился таймер.
        /// </summary>
        public bool Recompute()
        {
            if (Compute == null) return false;

            bool newValue;
            try
            {
                newValue = Compute();
            }
            catch
            {
                return false;
            }

            // Если ничего не поменялось — выходим
            if (newValue == Value)
            {
                // Если мы передумали (сигнал скакнул обратно), сбрасываем ожидание
                lock (_lock) _hasPending = false;
                return false;
            }

            // Определяем задержку
            var delay = newValue ? OnDelay : OffDelay;

            // 1. МГНОВЕННОЕ ИЗМЕНЕНИЕ
            if (delay <= TimeSpan.Zero)
            {
                lock (_lock) _hasPending = false;
                return UpdateValue(newValue);
            }

            // 2. ОТЛОЖЕННОЕ ИЗМЕНЕНИЕ
            lock (_lock)
            {
                // Запоминаем, что мы хотим установить
                _pendingValue = newValue;
                _hasPending = true;
            }

            var ms = delay.TotalMilliseconds;
            if (ms < 0 || ms > int.MaxValue) ms = 0;

            // Запускаем таймер в фоне
            Task.Delay((int)ms).ContinueWith(_ =>
            {
                // Когда таймер вышел, сообщаем Контексту, что пора проверить
                // (Контекст сам вызовет ApplyPending)
                DelayedUpdateReady?.Invoke(this);
            });

            return false; // Сейчас значение еще старое
        }

        // === РАСЧЕТ СУММАРНОЙ ЗАДЕРЖКИ (ОПТИМИЗИРОВАННЫЙ) ===

        public TimeSpan GetTotalOnDelay()
        {
            // Создаем временную память (кэш) для одного расчета
            // Это решает проблему "зависания", когда узлы считаются по 1000 раз
            var cache = new Dictionary<string, TimeSpan>();
            return GetOnDelayRecursive(this, 0, cache);
        }

        private TimeSpan GetOnDelayRecursive(Node node, int depth, Dictionary<string, TimeSpan> cache)
        {
            // 1. Ограничитель глубины (снизил до 15, этого за глаза)
            if (depth > 15) return TimeSpan.Zero;

            // 2. Если мы уже считали эту ноду в этом проходе — возвращаем готовое
            if (cache.TryGetValue(node.Id, out var cachedVal)) return cachedVal;

            // 3. Своя задержка
            var myDelay = node.OnDelay.TotalMilliseconds > 50 ? node.OnDelay : TimeSpan.Zero;
            TimeSpan maxParentDelay = TimeSpan.Zero;

            if (node.LogicSource != null && node.LogicSource.Groups != null)
            {
                foreach (var group in node.LogicSource.Groups)
                {
                    if (group == null) continue;
                    foreach (var parent in group)
                    {
                        // !!! ВАЖНАЯ ОПТИМИЗАЦИЯ !!!
                        // Считаем накопленную задержку только по тем путям, где ТЕЧЕТ ТОК (Value = True).
                        // Зачем нам знать задержку пути, который выключен?
                        if (!parent.Value) continue;

                        var pDelay = GetOnDelayRecursive(parent, depth + 1, cache);
                        if (pDelay > maxParentDelay) maxParentDelay = pDelay;
                    }
                }
            }

            var result = maxParentDelay + myDelay;

            // Запоминаем результат
            cache[node.Id] = result;
            return result;
        }

        // --- То же самое для Off Delay ---

        public TimeSpan GetTotalOffDelay()
        {
            var cache = new Dictionary<string, TimeSpan>();
            return GetOffDelayRecursive(this, 0, cache);
        }

        private TimeSpan GetOffDelayRecursive(Node node, int depth, Dictionary<string, TimeSpan> cache)
        {
            if (depth > 15) return TimeSpan.Zero;
            if (cache.TryGetValue(node.Id, out var cachedVal)) return cachedVal;

            var myDelay = node.OffDelay.TotalMilliseconds > 50 ? node.OffDelay : TimeSpan.Zero;
            TimeSpan maxParentDelay = TimeSpan.Zero;

            if (node.LogicSource != null && node.LogicSource.Groups != null)
            {
                foreach (var group in node.LogicSource.Groups)
                {
                    if (group == null) continue;
                    foreach (var parent in group)
                    {
                        // Для OFF задержки логика может быть сложнее, но обычно
                        // мы смотрим задержку того, кто сейчас держит сигнал.
                        // Если хотите считать по всем — уберите if, но с кэшем это будет работать быстро.
                        // if (parent.Value) continue; // (Тут логика зависит от типа элемента И/ИЛИ)

                        var pDelay = GetOffDelayRecursive(parent, depth + 1, cache);
                        if (pDelay > maxParentDelay) maxParentDelay = pDelay;
                    }
                }
            }

            var result = maxParentDelay + myDelay;
            cache[node.Id] = result;
            return result;
        }

        public override string ToString() => $"{Id} = {Value}";

        /// <summary>
        /// Вызывается Контекстом, когда таймер истек.
        /// </summary>
        public bool ApplyPending()
        {
            lock (_lock)
            {
                if (!_hasPending) return false;

                // Проверяем, актуально ли еще это изменение
                // (Вдруг за время таймера вход снова изменился?)
                // В данном случае мы просто применяем то, что насчитали ранее, 
                // или пересчитываем Compute() заново?
                // По правилам автоматики лучше перепроверить текущее состояние:
                bool currentCalc = Compute();
                if (currentCalc != _pendingValue)
                {
                    // "Дребезг": пока ждали таймер, условие ушло. Отмена.
                    _hasPending = false;
                    return false;
                }

                _hasPending = false;
                return UpdateValue(_pendingValue);
            }
        }

        private bool UpdateValue(bool newValue)
        {
            if (Value == newValue) return false;

            Value = newValue;
            Changed?.Invoke(this);
            return true;
        }
    }
}