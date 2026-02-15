using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using BPO_ex4.StationLogic;

namespace BPO_ex4.Visuals
{
    public class LightViewModel : VisualObjectViewModel
    {
        public int Number { get; set; } // number="1" из XML
        public string Direction { get; set; } // "odd" (нечет) / "even" (чет)

        // Список ламп: (Переменная логики, Цвет этой лампы)
        private List<(Node Node, SignalColor Color)> _lamps = new List<(Node, SignalColor)>();

        public double Width { get; set; } = 20;
        public double Height { get; set; } = 20;

        // Текущий цвет (привязан к View)
        private Brush _currentBrush = Brushes.DarkGray;
        public Brush FillColor
        {
            get => _currentBrush;
            set { _currentBrush = value; RaisePropertyChanged(nameof(FillColor)); }
        }

        public LightViewModel(double x, double y, int number, string name, string dir)
        {
            X = x; Y = y;
            Number = number;
            Name = name;
            Direction = dir;
            ZIndex = 30; // Светофоры рисуем поверх путей
        }

        public override void BindToLogic(Context ctx, SimulationEngine engine)
        {
            _engine = engine;

            // 1. Получаем порядок цветов из Excel
            if (!ctx.LightConfigs.TryGetValue(Name, out var colorOrder))
            {
                return;
            }

            // 2. Создаем привязки
            for (int i = 0; i < colorOrder.Count; i++)
            {
                int lampIndex = i + 1;             // 1, 2, 3...
                SignalColor color = colorOrder[i]; // Цвет

                // Формируем префикс, чтобы отличить лампу 1 от лампы 2 (OKSE_L1, OKSE_L2...)
                string searchPrefix = $"OKSE_L{lampIndex}_1";

                // САМЫЙ НАДЕЖНЫЙ ПОИСК:
                // 1. ID должен начинаться на OKSE_L{номер} (чтобы не перепутать цвета)
                // 2. Description не равен NULL (чтобы не упало)
                // 3. Description содержит имя ("БК-22")

                var node = ctx.GetAllNodes().FirstOrDefault(n =>
                    n.Id.StartsWith(searchPrefix) &&       // Фильтр по номеру лампы
                    n.Description != null &&               // ЗАЩИТА ОТ ВЫЛЕТА
                    n.Description.Contains(Name)           // Привязка к светофору
                );

                if (node != null)
                {
                    _lamps.Add((node, color));
                    node.Changed += _ => UpdateState();
                }
            }

            UpdateState();
        }

        private void UpdateState()
        {
            // 3. ОПРЕДЕЛЕНИЕ ЦВЕТА
            // Берем первую активную лампу
            var activeLamp = _lamps.FirstOrDefault(x => x.Node.Value);

            if (activeLamp.Node != null)
            {
                FillColor = activeLamp.Color.ToBrush();
            }
            else
            {
                // Выключен (Темно-серый)
                FillColor = new SolidColorBrush(Color.FromArgb(255, 40, 40, 40));
            }

            RaisePropertyChanged(null);
        }

        protected override void OnLogicChanged()
        {
            UpdateState();
        }
    }
}