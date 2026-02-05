using System.Collections.Generic;

namespace BPO_ex4.StationLogic
{
    public class SimulationEngine
    {
        private Context _ctx;

        // Очередь "грязных" нод, которые требуют пересчета
        private Queue<Node> _dirtyNodes = new Queue<Node>();

        public SimulationEngine(Context ctx)
        {
            _ctx = ctx;
        }

        // Этот метод вызывается, когда пользователь (или таймер) меняет значение
        public void InjectChange(Node node, bool newValue)
        {
            if (node.Value != newValue)
            {
                node.Value = newValue;
                // Добавляем всех зависимых в очередь на пересчет
                NotifyDependents(node);
                // Запускаем волну
                Propagate();
            }
        }

        // Главный цикл распространения сигнала (работает только пока есть изменения)
        private void Propagate()
        {
            // Пока очередь не пуста — обрабатываем
            // (В сложных системах тут нужен топологический сортировщик, но для релейной логики хватит FIFO)
            while (_dirtyNodes.Count > 0)
            {
                var node = _dirtyNodes.Dequeue();

                // 1. Запоминаем старое
                bool oldVal = node.Value;

                // 2. Считаем новое (выполняем формулу)
                bool newVal = node.Compute();

                // 3. Если изменилось — распространяем волну дальше
                if (oldVal != newVal)
                {
                    node.Value = newVal;
                    NotifyDependents(node);
                }
                // Если не изменилось — волна тут умирает, дальше не идем
            }
        }

        private void NotifyDependents(Node node)
        {
            foreach (var dependent in node.Dependents)
            {
                // Добавляем в очередь, если её там еще нет (чтобы не считать дважды)
                if (!_dirtyNodes.Contains(dependent))
                {
                    _dirtyNodes.Enqueue(dependent);
                }
            }
        }
    }
}