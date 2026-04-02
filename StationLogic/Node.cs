using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;

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
                    RaiseChanged();
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

        // === НОВОЕ: Защита от таймеров-призраков ===
        private long _timerVersion = 0;

        // === ГЛАВНЫЙ МЕТОД ПЕРЕСЧЕТА ===
        public bool Recompute()
        {
            if (Compute == null) return false;

            bool newValue;
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
                        _timerVersion++;     // УБИВАЕМ СТАРЫЕ ФОНОВЫЕ ТАЙМЕРЫ!
                    }
                }
                return false; // Изменений нет
            }

            // 2. Определяем задержку
            // Если в самом узле задержка 0, пытаемся вытащить её напрямую из исходного класса логики
            TimeSpan actualOnDelay = this.OnDelay > TimeSpan.Zero ? this.OnDelay : (LogicSource?.OnDelay ?? TimeSpan.Zero);
            TimeSpan actualOffDelay = this.OffDelay > TimeSpan.Zero ? this.OffDelay : (LogicSource?.OffDelay ?? TimeSpan.Zero);

            var delay = newValue ? actualOnDelay : actualOffDelay;

            // 3. МГНОВЕННОЕ ИЗМЕНЕНИЕ (Нет задержки)
            if (delay <= TimeSpan.Zero)
            {
                lock (_lock)
                {
                    _hasPending = false;
                    _timerVersion++; // На всякий случай отменяем все старые задержки
                }

                bool changed = UpdateValue(newValue);
                if (changed)
                {
                    AppLogger.Log($"AUTO: {Id} -> {newValue}");
                }
                return changed;
            }

            // 4. ЗАПУСК ТАЙМЕРА
            long myVersion; // Запоминаем версию именно ЭТОГО таймера
            lock (_lock)
            {
                if (_hasPending && _pendingValue == newValue)
                    return false; // Такой таймер уже тикает

                _pendingValue = newValue;
                _hasPending = true;

                _timerVersion++; // Генерируем уникальный ID для нового таймера
                myVersion = _timerVersion;
            }

            var ms = delay.TotalMilliseconds;
            if (ms < 0 || ms > int.MaxValue) ms = 0;

            Task.Delay((int)ms).ContinueWith(_ =>
            {
                lock (_lock)
                {
                    // Самая важная проверка!
                    // Если с момента старта этого Delay версия изменилась (был сброс или новый старт)
                    if (!_hasPending || _timerVersion != myVersion)
                        return; // ... то это таймер-призрак. Тихо выходим, не трогая станцию!
                }

                // Иначе всё ок, дергаем ядро
                DelayedUpdateReady?.Invoke(this);
            });

            return false;
        }

        // 2.04 ApplyPending
        public bool ApplyPending()
        {
            bool valueToApply;

            // 1. Хватаем замок ТОЛЬКО чтобы забрать значение и сбросить флаг
            lock (_lock)
            {
                if (!_hasPending) return false;
                _hasPending = false;
                valueToApply = _pendingValue;
            }

            // 2. Спокойно применяем значение без блокировки потоков
            bool changed = UpdateValue(valueToApply);
            if (changed)
            {
                AppLogger.Log($"TIMER TICK: {Id} {Description}-> {Value}");
            }
            return changed;
        }

        private bool UpdateValue(bool newValue)
        {
            if (Value == newValue) return false;
            Value = newValue;
            return true;
        }

        private void RaiseChanged()
        {
            var handler = Changed;
            if (handler == null) return;

            var app = Application.Current;
            if (app != null && !app.Dispatcher.CheckAccess())
            {
                app.Dispatcher.Invoke(() => handler(this));
            }
            else
            {
                handler(this);
            }
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