using System.Linq;
using System.Windows.Media;
using BPO_ex4.StationLogic;

namespace BPO_ex4.Visuals
{
    public class SwitchViewModel : VisualObjectViewModel
    {
        // === ПЕРЕМЕННЫЕ ДЛЯ ЧТЕНИЯ (СТАТУС) ===
        private Node _pkNode; // Плюс Контроль (SWITCH_PK)
        private Node _mkNode; // Минус Контроль (SWITCH_MK)

        // === ПЕРЕМЕННЫЕ ДЛЯ ЗАПИСИ (УПРАВЛЕНИЕ) ===
        private Node _inPlusNode;  // Управление плюсом (SWITCH_IN[0])
        private Node _inMinusNode; // Управление минусом (SWITCH_IN[1])

        public double Width { get; set; } = 20;
        public double Height { get; set; } = 20;

        public Brush FillColor
        {
            get
            {
                // Если не нашли переменные контроля - Серый (или Синий)
                if (_pkNode == null || _mkNode == null) return Brushes.Gray;

                bool pk = _pkNode.Value;
                bool mk = _mkNode.Value;

                // ЛОГИКА ЦВЕТОВ СТРЕЛКИ
                if (pk && !mk) return Brushes.Lime;      // Плюс (Зеленый)
                if (!pk && mk) return Brushes.Yellow;    // Минус (Желтый)
                if (!pk && !mk) return Brushes.Red;      // Нет контроля (Красный)

                return Brushes.Violet;                   // Взрез/Ошибка (оба активны)
            }
        }

        public SwitchViewModel(double x, double y, string name)
        {
            X = x; Y = y; Name = name; ZIndex = 10; // Стрелка поверх путей
        }

        public override void BindToLogic(Context ctx, SimulationEngine engine)
        {
            _engine = engine;

            // 1. ИЩЕМ СТАТУС (PK и MK) по описанию (напр. "1", "5", "11")
            _pkNode = ctx.GetAllNodes().FirstOrDefault(n => n.Id.StartsWith("SWITCH_PK") && n.Description == Name);
            _mkNode = ctx.GetAllNodes().FirstOrDefault(n => n.Id.StartsWith("SWITCH_MK") && n.Description == Name);

            // Подписываемся на изменение статусов (чтобы менять цвет)
            if (_pkNode != null) _pkNode.Changed += (n) => OnLogicChanged();
            if (_mkNode != null) _mkNode.Changed += (n) => OnLogicChanged();

            // 2. ИЩЕМ УПРАВЛЕНИЕ (IN)
            // Они спрятаны внутри главной переменной SWITCH (или внутри самих PK/MK как родители)
            // Самый надежный способ, если SWITCH_IN не имеет Description:
            // Найти объект "SWITCH" с таким же Description, и взять его входы.

            var mainSwitch = ctx.GetAllNodes().FirstOrDefault(n => n.Id.StartsWith("SWITCH") && !n.Id.Contains("_") && n.Description == Name);

            if (mainSwitch != null && mainSwitch.LogicSource?.Groups != null)
            {
                // Ищем внутри входов
                foreach (var group in mainSwitch.LogicSource.Groups)
                {
                    if (group == null) continue;
                    foreach (var input in group)
                    {
                        if (input.Id.StartsWith("SWITCH_IN"))
                        {
                            // Обычно порядок такой: Сначала Плюс, потом Минус.
                            // Если будет работать наоборот - поменяем местами тут.
                            if (_inPlusNode == null) _inPlusNode = input;
                            else if (_inMinusNode == null) _inMinusNode = input;
                        }
                    }
                }
            }

            // Сразу обновляем цвет
            OnLogicChanged();
        }

        // === ПРАВЫЙ КЛИК: ПЕРЕВОД СТРЕЛКИ ===
        protected override void OnRightClick()
        {
            if (_engine == null || _inPlusNode == null || _inMinusNode == null) return;

            // Смотрим текущее состояние ПО КОНТРОЛЮ (PK/MK)
            bool isPlus = _pkNode != null && _pkNode.Value;

            if (isPlus)
            {
                // Если сейчас Плюс -> переводим в Минус
                AppLogger.Log($"SWITCH {Name}: Перевод в МИНУС");
                _engine.InjectChange(_inPlusNode, false);  // Плюс выкл
                _engine.InjectChange(_inMinusNode, true);  // Минус вкл
            }
            else
            {
                // Если Минус или нет контроля -> переводим в Плюс
                AppLogger.Log($"SWITCH {Name}: Перевод в ПЛЮС");
                _engine.InjectChange(_inPlusNode, true);   // Плюс вкл
                _engine.InjectChange(_inMinusNode, false); // Минус выкл
            }
        }

        // === ЛЕВЫЙ КЛИК: ПОТЕРЯ КОНТРОЛЯ ===
        protected override void OnLeftClick()
        {
            if (_engine == null) return;

            AppLogger.Log($"SWITCH {Name}: Имитация потери контроля");

            // Выключаем оба управляющих реле
            if (_inPlusNode != null) _engine.InjectChange(_inPlusNode, false);
            if (_inMinusNode != null) _engine.InjectChange(_inMinusNode, false);
        }

        protected override void OnLogicChanged()
        {
            RaisePropertyChanged(null); // Перерисовать цвет
        }
    }
}