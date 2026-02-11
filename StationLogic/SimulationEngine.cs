using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace BPO_ex4.StationLogic
{
    public class SimulationEngine
    {
        private Queue<Node> _queue = new Queue<Node>();
        public event Action UIUpdateRequested;

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

        public void OnDelayedUpdateReady(Node node)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // Применяем изменение (лог напишется внутри ApplyPending)
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

                // Recompute теперь сам пишет лог AUTO или START TIMER
                // Если он вернул true, значит значение изменилось МГНОВЕННО, и надо идти дальше.
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