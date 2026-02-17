using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using BPO_ex4.StationLogic;

namespace BPO_ex4.Visuals
{
    public class LightViewModel : VisualObjectViewModel
    {
        public int Number { get; set; }
        public string Direction { get; set; }

        // 1. Лампы (OKSE -> Цвет)
        private List<(Node Node, SignalColor Color)> _lamps = new List<(Node, SignalColor)>();

        // 2. Огневые реле (Цвет -> SIGNAL_OUT_L)
        private Dictionary<SignalColor, Node> _controlNodes = new Dictionary<SignalColor, Node>();

        public double Width { get; set; } = 20;
        public double Height { get; set; } = 20;

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
            ZIndex = 30;
        }

        public override void BindToLogic(Context ctx, SimulationEngine engine)
        {
            _engine = engine;
            _lamps.Clear();
            _controlNodes.Clear();

            // 1. Получаем список цветов. 
            // ТАК КАК У ТЕБЯ List<SignalColor>, МЫ БЕРЕМ ЕГО НАПРЯМУЮ
            if (!ctx.LightConfigs.TryGetValue(Name, out List<SignalColor> colorOrder))
            {
                return;
            }

            for (int i = 0; i < colorOrder.Count; i++)
            {
                int lampIndex = i + 1;
                SignalColor color = colorOrder[i];

                // =========================================================
                // А. ПРИВЯЗКА OKSE (ВХОД: Лампа горит?)
                // =========================================================
                // Так как ProtoNumber у нас нет, ищем по префиксу и Description
                string searchPrefix = $"OKSE_L{lampIndex}_1";

                var okseNode = ctx.GetAllNodes().FirstOrDefault(n =>
                    n.Id.StartsWith(searchPrefix) &&
                    n.Description != null &&
                    n.Description.Contains($"({Name})")
                );

                if (okseNode != null)
                {
                    _lamps.Add((okseNode, color));
                    // При изменении OKSE вызываем пересчет
                    okseNode.Changed += _ => UpdateState();
                }

                // =========================================================
                // Б. ПРИВЯЗКА SIGNAL_OUT_L (ВЫХОД: Огневое реле)
                // =========================================================
                string logicPrefix = GetLogicPrefixForColor(color);

                if (logicPrefix != null)
                {
                    // Ищем "родительскую" переменную (напр. SIGNAL_SO)
                    var logicNode = ctx.GetAllNodes().FirstOrDefault(n =>
                        n.Id.StartsWith(logicPrefix) &&
                        n.Description != null &&
                        n.Description.Contains(Name)
                    );

                    // Лезем в её зависимости
                    if (logicNode != null && logicNode.LogicSource?.Groups != null)
                    {
                        foreach (var group in logicNode.LogicSource.Groups)
                        {
                            if (group == null) continue;

                            var input = group.FirstOrDefault(n => n.Id.StartsWith("SIGNAL_OUT_L"));
                            if (input != null)
                            {
                                // Нашли огневое реле -> запомнили
                                _controlNodes[color] = input;
                                break;
                            }
                        }
                    }
                }
            }

            // Первичное обновление
            UpdateState();
        }

        private string GetLogicPrefixForColor(SignalColor color)
        {
            switch (color)
            {
                case SignalColor.Blue: return "SIGNAL_SO";   // Синий
                case SignalColor.Green: return "SIGNAL_zO";   // Зеленый
                case SignalColor.Yellow: return "SIGNAL_1ZhO"; // Желтый
                case SignalColor.Red: return "SIGNAL_KO";   // Красный
                case SignalColor.White: return "SIGNAL_BO";   // Белый
                default: return null;
            }
        }

        private void UpdateState()
        {
            if (_engine == null) return;

            // --- 1. СИНХРОНИЗАЦИЯ (ОБРАТНАЯ СВЯЗЬ) ---
            foreach (var lampInfo in _lamps)
            {
                Node okse = lampInfo.Node;       // Лампа (Вход)
                SignalColor color = lampInfo.Color; // Цвет

                // Если есть связь с огневым реле
                if (_controlNodes.TryGetValue(color, out Node relayNode))
                {
                    bool shouldBeOn = okse.Value;
                    if (relayNode.Value != shouldBeOn)
                    {
                        _engine.InjectChange(relayNode, shouldBeOn);
                    }
                }
            }

            // --- 2. ОТРИСОВКА ---
            var activeLamp = _lamps.LastOrDefault(x => x.Node.Value);

            if (activeLamp.Node != null)
            {
                FillColor = activeLamp.Color.ToBrush();
            }
            else
            {
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