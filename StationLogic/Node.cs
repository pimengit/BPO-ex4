using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BPO_ex4.StationLogic
{
    public class Node
    {
        public string Id { get; set; }
        public string Description { get; set; }
        private bool _value;
        public bool Value
        {
            get => _value;
            set
            {
                if (_value != value)
                {
                    _value = value;
                    // ТЕПЕРЬ ЛЮБОЕ ИЗМЕНЕНИЕ ВЫЗЫВАЕТ СОБЫТИЕ!
                    // И график сразу узнает об этом.
                    Changed?.Invoke(this);
                }
            }
        }
        public SheetLogic LogicSource { get; set; }

        public List<Node> Dependents { get; set; } = new List<Node>();
        public Func<bool> Compute { get; set; } = () => false;

        public event Action<Node>? Changed;
        public event Action<Node>? DelayedUpdateReady;

        public TimeSpan OnDelay { get; set; } = TimeSpan.Zero;
        public TimeSpan OffDelay { get; set; } = TimeSpan.Zero;

        private bool _pendingValue;
        private bool _hasPending;
        private object _lock = new object();

        // === ГЛАВНЫЙ МЕТОД ПЕРЕСЧЕТА ===
        public bool Recompute()
        {
            if (Compute == null) return false;

            bool newValue;
            // Считаем новое значение по формуле
            try
            {
                newValue = Compute();
            }
            catch (Exception ex)
            {
                AppLogger.Log($"ERROR calc {Id}: {ex.Message}");
                return false;
            }

            // 1. Если значение такое же, как было
            if (newValue == Value)
            {
                lock (_lock)
                {
                    if (_hasPending)
                    {
                        _hasPending = false; // Сброс таймера (дребезг)
                        //AppLogger.Log($"CANCEL TIMER: {Id} (signal lost)");
                    }
                }
                return false; // Изменений нет
            }

            // 2. Определяем задержку
            var delay = newValue ? OnDelay : OffDelay;

            // 3. МГНОВЕННОЕ ИЗМЕНЕНИЕ (Нет задержки)
            if (delay <= TimeSpan.Zero)
            {
                lock (_lock) _hasPending = false;
                // Меняем значение сразу
                bool changed = UpdateValue(newValue);
                if (changed)
                {
                    // ЛОГ ПИШЕМ ПРЯМО ТУТ!
                    AppLogger.Log($"AUTO: {Id} -> {newValue}");
                }
                return changed;
            }

            // 4. ЗАПУСК ТАЙМЕРА
            lock (_lock)
            {
                if (_hasPending && _pendingValue == newValue)
                    return false; // Таймер уже тикает

                _pendingValue = newValue;
                _hasPending = true;
            }

            // ЛОГ О ЗАПУСКЕ ТАЙМЕРА
            //AppLogger.Log($"START TIMER: {Id} -> {newValue} ({delay.TotalSeconds}s)");

            var ms = delay.TotalMilliseconds;
            if (ms < 0 || ms > int.MaxValue) ms = 0;

            Task.Delay((int)ms).ContinueWith(_ =>
            {
                DelayedUpdateReady?.Invoke(this);
            });

            // Возвращаем false, так как ПРЯМО СЕЙЧАС значение еще старое
            return false;
        }

        public bool ApplyPending()
        {
            lock (_lock)
            {
                if (!_hasPending) return false;
                _hasPending = false;

                // Применяем отложенное значение
                bool changed = UpdateValue(_pendingValue);
                if (changed)
                {
                    // ЛОГ СРАБАТЫВАНИЯ ТАЙМЕРА
                    AppLogger.Log($"TIMER TICK: {Id} {Description}-> {Value}");
                }
                return changed;
            }
        }

        private bool UpdateValue(bool newValue)
        {
            if (Value == newValue) return false;
            Value = newValue;
            Changed?.Invoke(this); // Уведомляем движок
            return true;
        }

        // Таймеры (кэш)
        public TimeSpan GetTotalOnDelay() => GetOnDelayRecursive(this, 0, new Dictionary<string, TimeSpan>());
        public TimeSpan GetTotalOffDelay() => GetOffDelayRecursive(this, 0, new Dictionary<string, TimeSpan>());

        private TimeSpan GetOnDelayRecursive(Node node, int depth, Dictionary<string, TimeSpan> cache)
        {
            if (depth > 15) return TimeSpan.Zero;
            if (cache.TryGetValue(node.Id, out var val)) return val;
            var myDelay = node.OnDelay.TotalMilliseconds > 50 ? node.OnDelay : TimeSpan.Zero;
            TimeSpan maxParent = TimeSpan.Zero;
            if (node.LogicSource?.Groups != null)
                foreach (var group in node.LogicSource.Groups)
                    if (group != null)
                        foreach (var parent in group)
                            if (parent.Value)
                            {
                                var pDelay = GetOnDelayRecursive(parent, depth + 1, cache);
                                if (pDelay > maxParent) maxParent = pDelay;
                            }
            cache[node.Id] = maxParent + myDelay;
            return maxParent + myDelay;
        }

        private TimeSpan GetOffDelayRecursive(Node node, int depth, Dictionary<string, TimeSpan> cache)
        {
            if (depth > 15) return TimeSpan.Zero;
            if (cache.TryGetValue(node.Id, out var val)) return val;
            var myDelay = node.OffDelay.TotalMilliseconds > 50 ? node.OffDelay : TimeSpan.Zero;
            TimeSpan maxParent = TimeSpan.Zero;
            if (node.LogicSource?.Groups != null)
                foreach (var group in node.LogicSource.Groups)
                    if (group != null)
                        foreach (var parent in group)
                        {
                            var pDelay = GetOffDelayRecursive(parent, depth + 1, cache);
                            if (pDelay > maxParent) maxParent = pDelay;
                        }
            cache[node.Id] = maxParent + myDelay;
            return maxParent + myDelay;
        }

        public override string ToString() => $"{Id} = {Value}";
    }
}