using System;
using System.Collections.Generic;
using System.Linq; // <--- Нужно для ToList()
using System.Windows.Threading;

namespace BPO_ex4.StationLogic
{
    public class SimulationEngine
    {
        private Queue<Node> _queue = new Queue<Node>();

        // 1. Добавляем поле для контекста
        private Context _context;

        public event Action UIUpdateRequested;

        // 2. Конструктор, принимающий Context
        public SimulationEngine(Context ctx)
        {
            _context = ctx;
        }

        public void InjectChange(Node node, bool newValue)
        {
            if (node.Value != newValue)
            {
                node.Value = newValue;
                AppLogger.Log($"MANUAL: {node.Id} -> {newValue}");

                Propagate(node);
                ForceUpdateUI();
            }
        }

        // Полный пересчет ВСЕХ переменных
        /*public void ForceRecomputeAll()
        {
            if (_context == null) return;

            // Теперь _context доступен
            var allNodes = _context.GetAllNodes().ToList();
            bool anyChanged = true;
            int safetyCounter = 0;
            int maxIterations = 10;

            AppLogger.Log("=== FORCE RECOMPUTE ALL STARTED ===");

            while (anyChanged && safetyCounter < maxIterations)
            {
                anyChanged = false;
                safetyCounter++;

                foreach (var node in allNodes)
                {
                    // Вызываем пересчет
                    if (node.Recompute())
                    {
                        anyChanged = true;
                        // Если значение изменилось, сразу кидаем зависимых в очередь и обрабатываем
                        // Это сделает пересчет более "честным", распространяя волну сразу
                        Propagate(node);
                    }
                }
            }

            ForceUpdateUI();
            AppLogger.Log($"=== FORCE RECOMPUTE FINISHED (Iterations: {safetyCounter}) ===");
        }*/

        public void OnDelayedUpdateReady(Node node)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (node.ApplyPending())
                {
                    Propagate(node);
                    ForceUpdateUI();
                }
            });
        }

        private void Propagate(Node startNode)
        {
            EnqueueDependents(startNode);

            while (_queue.Count > 0)
            {
                var node = _queue.Dequeue();
                if (node.Recompute())
                {
                    EnqueueDependents(node);
                }
            }
        }

        private void EnqueueDependents(Node node)
        {
            if (node.Dependents == null) return;
            foreach (var dep in node.Dependents)
            {
                if (!_queue.Contains(dep)) _queue.Enqueue(dep);
            }
        }

        private void ForceUpdateUI()
        {
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                UIUpdateRequested?.Invoke();
            }, DispatcherPriority.Render);
        }
    }
}