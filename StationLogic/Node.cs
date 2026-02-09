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

        public TimeSpan GetTotalOnDelay(int depth = 0)
        {
            if (depth > 20) return TimeSpan.Zero; // Защита от бесконечной рекурсии (циклов)

            // 1. Моя задержка (учитываем, если > 50мс)
            var myDelay = OnDelay.TotalMilliseconds > 50 ? OnDelay : TimeSpan.Zero;

            // 2. Ищем максимальную задержку среди родителей, которые сейчас TRUE (активный путь)
            //    (Или можно брать всех родителей, зависит от логики. Обычно смотрят активный путь)
            TimeSpan maxParentDelay = TimeSpan.Zero;

            if (LogicSource != null && LogicSource.Groups != null)
            {
                foreach (var group in LogicSource.Groups)
                {
                    if (group == null) continue;
                    foreach (var parent in group)
                    {
                        // Если вы хотите суммировать ВСЕ пути, уберите проверку parent.Value
                        // Если только активный путь сигнала - оставьте.
                        // Для надежности пока берем просто максимум из всех родителей:
                        var pDelay = parent.GetTotalOnDelay(depth + 1);
                        if (pDelay > maxParentDelay) maxParentDelay = pDelay;
                    }
                }
            }

            return maxParentDelay + myDelay;
        }

        public TimeSpan GetTotalOffDelay(int depth = 0)
        {
            if (depth > 20) return TimeSpan.Zero;

            var myDelay = OffDelay.TotalMilliseconds > 50 ? OffDelay : TimeSpan.Zero;
            TimeSpan maxParentDelay = TimeSpan.Zero;

            if (LogicSource != null && LogicSource.Groups != null)
            {
                foreach (var group in LogicSource.Groups)
                {
                    if (group == null) continue;
                    foreach (var parent in group)
                    {
                        var pDelay = parent.GetTotalOffDelay(depth + 1);
                        if (pDelay > maxParentDelay) maxParentDelay = pDelay;
                    }
                }
            }
            return maxParentDelay + myDelay;
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