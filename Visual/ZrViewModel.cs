using BPO_ex4.StationLogic;
using System.Windows.Media;

namespace BPO_ex4.Visuals
{
    public class ZrViewModel : VisualObjectViewModel
    {
        private List<Node> _zrNode = new List<Node>(); //Замыкающие реле//Искуственная разделка
        public double Width { get; set; }
        public double Height { get; set; } = 8;

        public Brush FillColor
        {
            get
            {
                // 1. Если нет основной привязки (SECT_IN) -> Синий (Ошибка)
                if (_node == null) return Brushes.Red;

                // 2. Если ЗАНЯТО (Value == false) -> Красный (Приоритет №1)
                // Даже если маршрут задан, под поездом секция должна быть красной
                //f (_zrNode.Count == 0) return Brushes.Green;
                bool allACtive = _zrNode.All(n => n.Value);
                // 4. Иначе -> Серый (Свободно)
                return allACtive ? Brushes.Orange : Brushes.Blue;
            }
        }

        public ZrViewModel(double x, double y, double w, string name)
        {
            X = x;
            Y = y;
            Width = w;
            Name = name;
        }

        public override void BindToLogicZr(Context ctx, SimulationEngine engine)
        {
            // 1. Вызываем базовую логику (она найдет _node = SECT_IN через SECT_P)
            base.BindToLogicZr(ctx, engine);
            // 2. Ищем SECT_PZ по описанию (оно совпадает с Name, например "360")
            var allPartRoutes = ctx.GetAllNodes().Where(n => n.Id.StartsWith("PARTROUTE_Zc"));

            foreach (var routeNode in allPartRoutes)
            {
                if (IsDependentOn(routeNode, _node))
                {
                    _zrNode.Add(routeNode);
                    // Подписываемся на их изменения!
                    routeNode.Changed += (n) => OnLogicChanged();
                }
            }

            // Первичное обновление
            OnLogicChanged();
        }

        private bool IsDependentOn(Node child, Node parent)
        {
            if (child.LogicSource?.Groups == null) return false;

            foreach (var group in child.LogicSource.Groups)
            {
                if (group == null) continue;
                // Если в группе входов есть наш родитель -> значит зависит!
                if (group.Contains(parent)) return true;
            }
            return false;
        }
        // === ИСПРАВЛЕНИЕ ТУТ ===
        protected override void OnLogicChanged()
        {
            // Используем метод родителя. 
            // Dispatcher уже внутри него, так что тут просто одна строчка.
            //RaisePropertyChanged(nameof(FillColor));
            // где-то (в OnLogicChanged)
            AppLogger.Log($"OnLogicChanged: {Name} -> FillColor now: {_node?.Value}");
            RaisePropertyChanged(null);
            //RaisePropertyChanged(nameof(FillColor));
        }
    }
}