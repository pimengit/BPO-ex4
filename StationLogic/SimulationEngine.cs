using System;
using System.Collections.Generic;
using System.Windows.Threading; // Для Dispatcher

namespace BPO_ex4.StationLogic
{
    public class SimulationEngine
    {
        // Очередь нод для пересчета
        private Queue<Node> _queue = new Queue<Node>();

        // Событие: "Движок закончил работу или сработал таймер — обновите экран!"
        public event Action UIUpdateRequested;

        // Конструктор больше не требует Context, так как работает с конкретными нодами
        public SimulationEngine() { }

        // 1. Входная точка (клик пользователя или логика вкладок)
        public void InjectChange(Node node, bool newValue)
        {
            // Если значение реально меняется — применяем и пускаем волну
            if (node.Value != newValue)
            {
                node.Value = newValue;
                AppLogger.Log($"MANUAL CHANGE: {node.Id} -> {newValue}");

                // Запускаем пересчет зависимых
                Propagate(node);
            }
        }

        // 2. Обработчик таймеров (подписывается в MainWindow)
        // Вызывается, когда Нода говорит: "Мой таймер истек, я готова измениться"
        public void OnDelayedUpdateReady(Node node)
        {
            // Возвращаемся в главный поток UI, чтобы безопасно менять значения
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // Применяем отложенное значение (метод ApplyPending сам проверит актуальность)
                if (node.ApplyPending())
                {
                    AppLogger.Log($"TIMER TICK: {node.Id} -> {node.Value}");

                    // Значение изменилось — пускаем волну дальше
                    Propagate(node);

                    // Просим UI перерисоваться (так как это произошло асинхронно)
                    UIUpdateRequested?.Invoke();
                }
            });
        }

        // 3. Волна распространения (BFS)
        private void Propagate(Node startNode)
        {
            // Добавляем всех, кто зависит от стартовой ноды
            EnqueueDependents(startNode);

            // Пока очередь не пуста
            while (_queue.Count > 0)
            {
                var node = _queue.Dequeue();

                // !!! ГЛАВНОЕ ОТЛИЧИЕ !!!
                // Используем Recompute(), а не Compute().
                // Recompute() внутри себя проверяет таймеры:
                // - Если задержки нет -> возвращает true (значение изменилось сразу).
                // - Если задержка есть -> запускает таймер и возвращает false (ждем).
                if (node.Recompute())
                {
                    // Если изменилось мгновенно — идем дальше по цепочке
                    EnqueueDependents(node);
                }
            }
        }

        private void EnqueueDependents(Node node)
        {
            if (node.Dependents == null) return;

            foreach (var dep in node.Dependents)
            {
                // Защита от дублей в очереди
                if (!_queue.Contains(dep))
                {
                    _queue.Enqueue(dep);
                }
            }
        }
    }
}