using BPO_ex4.StationLogic;
using System.Windows.Media;

namespace BPO_ex4.Visuals
{
    public class SectionViewModel : VisualObjectViewModel
    {
        private Node _pzNode; //зеленая полоса
        private Node _lzNode; //ложная занятость
        public double Width { get; set; }
        public double Height { get; set; } = 8;

        public Brush FillColor
        {
            get
            {
                // 1. Если нет основной привязки (SECT_IN) -> Синий (Ошибка)
                if (_node == null) return Brushes.Blue;

                // 2. Если ЗАНЯТО (Value == false) -> Красный (Приоритет №1)
                // Даже если маршрут задан, под поездом секция должна быть красной
                if (!_node.Value && !_lzNode.Value) return Brushes.Red;

                if (!_node.Value && _lzNode.Value) return Brushes.Yellow;

                // 3. Если МАРШРУТ (PZ == true) -> Зеленый (Приоритет №2)
                if (_pzNode != null && _pzNode.Value) return Brushes.Lime;


                // 4. Иначе -> Серый (Свободно)
                return Brushes.LightGray;
            }
        }

        public SectionViewModel(double x, double y, double w, string name)
        {
            X = x;
            Y = y;
            Width = w;
            Name = name;
        }




        public override void BindToLogicSect(Context ctx, SimulationEngine engine)
        {
            // 1. Вызываем базовую логику (она найдет _node = SECT_IN через SECT_P)
            base.BindToLogicSect(ctx, engine);

            // 2. Ищем SECT_PZ по описанию (оно совпадает с Name, например "360")
            _pzNode = ctx.GetAllNodes()
                         .FirstOrDefault(n => n.Id.StartsWith("SECT_Pz") && n.Description == Name);

            _lzNode = ctx.GetAllNodes()
             .FirstOrDefault(n => n.Id.StartsWith("SECT_Lz") && n.Description == Name);

            if (_pzNode != null)
            {
                // Подписываемся на изменения PZ тоже!
                _pzNode.Changed += (n) => OnLogicChanged();
            }

            if (_lzNode != null)
            {
                // Подписываемся на изменения PZ тоже!
                _lzNode.Changed += (n) => OnLogicChanged();
            }
            // Обновляем цвет сразу после привязки
            OnLogicChanged();
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