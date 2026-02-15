using System;
using System.Collections.Generic;
using System.Linq;

namespace BPO_ex4.StationLogic
{
    public class Context
    {
        private Dictionary<string, Node> nodes = new();

        // Очередь для обработки изменений (BFS подход, чтобы не было StackOverflow)
        private Queue<Node> _processingQueue = new();

        // Флаг, чтобы не запускать вложенные циклы ProcessQueue
        private bool _isProcessing = false;

        // Объект для блокировки (если таймеры сработают из другого потока)
        private object _queueLock = new object();

        public int GetAllNodesCount()
        {
            return nodes.Count;
        }

        public Dictionary<string, List<SignalColor>> LightConfigs { get; set; } = new Dictionary<string, List<SignalColor>>();

        /// <summary>
        /// Возвращает список всех узлов (понадобится для рисования диаграмм).
        /// </summary>
        public IEnumerable<Node> GetAllNodes()
        {
            return nodes.Values;
        }
        public Node Get(string id)
        {
            if (!nodes.TryGetValue(id, out var n))
            {
                n = new Node { Id = id };

                // Логирование изменений (можно раскомментировать для отладки)
                n.Changed += node =>
                {
                     Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] CHANGED: {node.Id} = {node.Value}");
                };

                // Обработка задержек (Task.Delay завершился и нода готова применить значение)
                n.DelayedUpdateReady += node =>
                {
                    lock (_queueLock)
                    {
                        // Пытаемся применить отложенное значение
                        if (node.ApplyPending())
                        {
                            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] TIMER APPLY: {node.Id} -> {node.Value}");

                            // Если значение изменилось -> добавляем зависимых в очередь
                            EnqueueDependents(node);

                            // Запускаем обработку очереди
                            ProcessQueue();
                        }
                    }
                };

                nodes[id] = n;
            }
            return n;
        }

        public void Set(string id, bool value)
        {
            lock (_queueLock)
            {
                var n = Get(id);

                // Принудительная установка значения (как физический вход)
                if (n.Value != value)
                {
                    // Меняем значение вручную (без задержек, т.к. это Set извне)
                    n.Value = value;
                    Console.WriteLine($"SET INPUT: {id} = {value}");

                    // Добавляем всех, кто зависит от этого узла, в очередь
                    EnqueueDependents(n);

                    // Запускаем волну вычислений
                    ProcessQueue();
                }
            }
        }

        public void RecomputeAll()
        {
            lock (_queueLock)
            {
                foreach (var n in nodes.Values)
                    _processingQueue.Enqueue(n);

                ProcessQueue();
            }
        }

        /// <summary>
        /// Добавляет всех "детей" узла в очередь на пересчет
        /// </summary>
        private void EnqueueDependents(Node n)
        {
            foreach (var dependent in n.Dependents)
            {
                _processingQueue.Enqueue(dependent);
            }
        }

        /// <summary>
        /// Главный цикл обработки графа (Итеративный, без рекурсии).
        /// </summary>
        private void ProcessQueue()
        {
            if (_isProcessing) return; // Уже работаем
            _isProcessing = true;

            try
            {
                int safetyCounter = 0;
                // Лимит шагов, чтобы остановить бесконечное мигание (триггеры)
                const int MAX_CYCLES = 200000;

                while (_processingQueue.Count > 0)
                {
                    var node = _processingQueue.Dequeue();

                    // Просим ноду пересчитаться (сама она никого не вызывает!)
                    bool changed = node.Recompute();

                    if (changed)
                    {
                        // Если она изменилась, ее соседи тоже могут измениться -> в очередь
                        EnqueueDependents(node);
                    }

                    // Защита от зависания (если логика "мигает" бесконечно)
                    safetyCounter++;
                    if (safetyCounter > MAX_CYCLES)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("!!! CRITICAL: Infinite logic loop detected (oscillation). Stopping queue. !!!");
                        Console.ResetColor();
                        _processingQueue.Clear();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in logic loop: {ex}");
            }
            finally
            {
                _isProcessing = false;
            }
        }

        // ==========================================
        //  DIAGNOSTICS & PRINTING (ВОССТАНОВЛЕНО)
        // ==========================================

        public void List(string prefix = null)
        {
            foreach (var n in nodes.Values.OrderBy(n => n.Id))
            {
                if (prefix == null || n.Id.StartsWith(prefix))
                    Console.WriteLine($"{n.Id} = {n.Value} ({n.Description})");
            }
        }

        public void Why(string id)
        {
            if (!nodes.TryGetValue(id, out var n))
            {
                Console.WriteLine("No such node");
                return;
            }

            Console.WriteLine($"{id} = {n.Value} ({n.Description})");
            Console.WriteLine("Depends on (parents):");

            // Ищем родителей (тех, у кого n в списке Dependents)
            foreach (var src in nodes.Values)
            {
                if (src.Dependents.Contains(n))
                    Console.WriteLine($"  <- {src.Id} = {src.Value} ({src.Description})");
            }
        }

        public void WhyAll(string id, int maxDepth = 3)
        {
            if (!nodes.TryGetValue(id, out var n))
            {
                Console.WriteLine("No such node");
                return;
            }

            Console.WriteLine($"=== UPSTREAM (Causes for {id}) ===");
            PrintUp(n, 0, maxDepth, new HashSet<Node>());

            Console.WriteLine($"=== DOWNSTREAM (Effects of {id}) ===");
            PrintDown(n, 0, maxDepth, new HashSet<Node>());
        }

        void PrintUp(Node n, int level, int maxDepth, HashSet<Node> seen)
        {
            if (level >= maxDepth) return;

            // Ищем всех, кто влияет на n (родителей)
            // Это медленно (перебор всего словаря), но для отладки OK
            foreach (var src in nodes.Values)
            {
                if (!src.Dependents.Contains(n))
                    continue;

                // Защита от зацикливания при печати
                if (!seen.Add(src))
                    continue;

                string indent = new string(' ', level * 2);
                Console.WriteLine($"{indent}<- {FormatNode(src)}");

                PrintUp(src, level + 1, maxDepth, seen);
            }
        }

        void PrintDown(Node n, int level, int maxDepth, HashSet<Node> seen)
        {
            if (level >= maxDepth) return;
            if (!seen.Add(n)) return;

            foreach (var d in n.Dependents)
            {
                // Для PrintDown мы уже добавили 'n' в seen, теперь проверяем 'd'
                // чтобы не уйти в вечный цикл A->B->A при печати
                if (seen.Contains(d)) continue;

                string indent = new string(' ', level * 2);
                Console.WriteLine($"{indent}-> {FormatNode(d)}");

                PrintDown(d, level + 1, maxDepth, seen);
            }
        }

        string FormatNode(Node n)
        {
            if (!string.IsNullOrEmpty(n.Description))
                return $"{n.Id} = {n.Value} ({n.Description})";

            return $"{n.Id} = {n.Value}";
        }
    }
}