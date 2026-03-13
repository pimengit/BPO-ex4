using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using BPO_ex4.StationLogic;
using System.Windows.Threading;

namespace BPO_ex4.Visuals
{
    public class LightViewModel : VisualObjectViewModel
    {
        public int Number { get; set; }
        public string Direction { get; set; }

        private Node _omoNode;

        private Node _psNode;
        private DispatcherTimer _blinkTimer;

        private bool _isRightPressed = false;       // Флаг: кнопка сейчас нажата?
        private bool _longPressHandled = false;

        // 1. Лампы (OKSE -> Цвет)
        private List<(Node Node, SignalColor Color)> _lamps = new List<(Node, SignalColor)>();

        // 2. Огневые реле (Цвет -> SIGNAL_OUT_L)
        private Dictionary<SignalColor, Node> _controlNodes = new Dictionary<SignalColor, Node>();

        public double Width { get; set; } = 20;
        public double Height { get; set; } = 20;

        private Brush _topColor = new SolidColorBrush(Color.FromArgb(255, 40, 40, 40));
        public Brush TopColor
        {
            get => _topColor;
            set { _topColor = value; RaisePropertyChanged(nameof(TopColor)); }
        }

        private Brush _bottomColor = new SolidColorBrush(Color.FromArgb(255, 40, 40, 40));
        public Brush BottomColor
        {
            get => _bottomColor;
            set { _bottomColor = value; RaisePropertyChanged(nameof(BottomColor)); }
        }

        // Полностью квалифицированное имя, чтобы не добавлять новые using
        private System.Windows.Visibility _bottomLensVisibility = System.Windows.Visibility.Collapsed;
        public System.Windows.Visibility BottomLensVisibility
        {
            get => _bottomLensVisibility;
            set { _bottomLensVisibility = value; RaisePropertyChanged(nameof(BottomLensVisibility)); }
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



            // ---------------------------------------------------------
            // ШАГ 2: Ищем переменную для КЛИКА (SECT_IN или RELAY_KRK)
            // ---------------------------------------------------------
            // Мы копаемся в исходниках SECT_P, чтобы найти, от чего она зависит
            var sigparentSwitch = ctx.GetAllNodes()
                                  .FirstOrDefault(n => n.Id.StartsWith("SIGNAL_OMO[") && n.Description.Contains(Name));

            if (sigparentSwitch != null && sigparentSwitch.LogicSource?.Groups != null)
            {
                foreach (var group in sigparentSwitch.LogicSource.Groups)
                {
                    if (group == null) continue;
                    foreach (var node in group)
                    {
                        if (node.Id.StartsWith("SIGNAL_OMO_DK"))
                        {
                            if (_omoNode == null) _omoNode = node;
                        }
                    }
                }
            }

            var sigparentPS = ctx.GetAllNodes()
                      .FirstOrDefault(n => n.Id.StartsWith("SIGNAL_PSK[") && n.Description.Contains(Name));

            if (sigparentPS != null && sigparentPS.LogicSource?.Groups != null)
            {
                foreach (var group in sigparentPS.LogicSource.Groups)
                {
                    if (group == null) continue;
                    foreach (var node in group)
                    {
                        if (node.Id.StartsWith("SIGNAL_PS_DK"))
                        {
                            if (_psNode == null) _psNode = node;
                        }
                    }
                }
            }


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
                case SignalColor.Yellow: return "SIGNAL_2ZhO"; // Желтый
                case SignalColor.Red: return "SIGNAL_KO";   // Красный
                case SignalColor.White: return "SIGNAL_BO";   // Белый
                case SignalColor.Violet: return "SIGNAL_PSO";   // Белый
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
            // Собираем все горящие лампы в список
            var activeLamps = _lamps.Where(x => x.Node.Value).ToList();
            var offColor = new SolidColorBrush(Color.FromArgb(255, 40, 40, 40));

            if (activeLamps.Count == 0)
            {
                // Ничего не горит: показываем один серый кружок
                TopColor = offColor;
                BottomLensVisibility = System.Windows.Visibility.Collapsed;
            }
            else if (activeLamps.Count == 1)
            {
                // Горит 1 огонь: красим верхний, нижний прячем
                TopColor = activeLamps[0].Color.ToBrush();
                BottomLensVisibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                // Горят 2 (или больше): красим оба, показываем нижний
                TopColor = activeLamps[0].Color.ToBrush();
                BottomColor = activeLamps[1].Color.ToBrush();
                BottomLensVisibility = System.Windows.Visibility.Visible;
            }

            RaisePropertyChanged(null);
        }

        protected override void OnLeftClick()
        {
            // Меняем состояние _node (это SECT_IN), а цвет перерисуется, когда движок пересчитает SECT_P
            if (_omoNode != null && _engine != null)
            {  
                _engine.InjectChange(_omoNode, true);
                System.Threading.Tasks.Task.Delay(1500).ContinueWith(_ =>
                {
                    _engine.InjectChange(_omoNode, false);
                });
            }
        }

        public void OnRightMouseDown()
        {
            if (_engine == null) return;

            // ЛОГИКА: Какую переменную включать при удержании?
            // Вариант А: Включаем Синий (или Белый), если он есть
            // Вариант Б: Если у тебя есть специальная кнопка пригласительного, ищи её здесь.

            // Пример: Берем кнопку для Синего (Blue)

                _engine.InjectChange(_psNode, true); // ВКЛЮЧАЕМ

            // Если синего нет, попробуем Белый

        }

        // Метод, когда отпустили ПКМ
        public void OnRightMouseUp()
        {
            if (_engine == null) return;

            _engine.InjectChange(_psNode, false); // ВКЛЮЧАЕМ
        }

        protected override void OnLogicChanged()
        {
            UpdateState();
        }
    }
}