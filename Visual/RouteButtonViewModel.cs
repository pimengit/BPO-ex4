using System.Linq;
using System.Text.RegularExpressions;
using BPO_ex4.StationLogic;

namespace BPO_ex4.Visuals
{
    public class RouteButtonViewModel : VisualObjectViewModel
    {
        private Node _dkNode;  // Кнопка установки (внутри KN)
        private Node _cancelDkNode; // Кнопка отмены (внутри KNO)
        private Context _context;   // Нужно сохранить контекст для поиска KNO

        public string ShortId { get; set; }
        public string Description { get; set; }

        // Цвет кнопки зависит от состояния самой ROUTE_KN
        public System.Windows.Media.Brush ButtonColor =>
            (_node != null && _node.Value) ? System.Windows.Media.Brushes.Lime : System.Windows.Media.Brushes.LightGray;

        public RouteButtonViewModel(Node node, SimulationEngine engine, Context ctx)
        {
            _node = node; // Это ROUTE_KN[...]
            _engine = engine;
            _context = ctx;

            Description = node.Description ?? "Нет описания";

            // Парсим номер (15) из ROUTE_KN[15]
            var match = Regex.Match(node.Id, @"(\d+)(?!.*\d)");
            ShortId = match.Success ? match.Value : "?";

            // 1. Ищем DK внутри самой KN (LogicSource -> Groups -> ROUTE_KN_DK)
            if (_node.LogicSource?.Groups != null)
            {
                foreach (var group in _node.LogicSource.Groups)
                {
                    if (group == null) continue;
                    var dk = group.FirstOrDefault(n => n.Id.StartsWith("ROUTE_KN_DK"));
                    if (dk != null)
                    {
                        _dkNode = dk;
                        break;
                    }
                }
            }

            // 2. Ищем KNO (Отмена) по ID. 
            // Если KN = ROUTE_KN[15], то KNO = ROUTE_KNO[15]
            if (match.Success)
            {
                string knoId = $"ROUTE_KNO[{match.Value}]";
                var knoNode = ctx.GetAllNodes().FirstOrDefault(n => n.Id == knoId);

                // Если нашли KNO, ищем внутри неё ROUTE_KNO_DK
                if (knoNode != null && knoNode.LogicSource?.Groups != null)
                {
                    foreach (var group in knoNode.LogicSource.Groups)
                    {
                        if (group == null) continue;
                        var cancelDk = group.FirstOrDefault(n => n.Id.StartsWith("ROUTE_KNO_DK"));
                        if (cancelDk != null)
                        {
                            _cancelDkNode = cancelDk;
                            break;
                        }
                    }
                }
            }

            // Подписка на изменение цвета (следим за состоянием KN)
            _node.Changed += (n) => RaisePropertyChanged(nameof(ButtonColor));
        }

        protected override void OnLeftClick()
        {
            if (_node == null || _engine == null) return;

            // Логика нажатия
            if (_node.Value == true)
            {
                // Сценарий ОТМЕНЫ (KN уже активна -> жмем KNO_DK)
                if (_cancelDkNode != null)
                {
                    PressButton(_cancelDkNode);
                }
                else
                {
                    // Лог или визуальная индикация ошибки (нет кнопки отмены)
                }
            }
            else
            {
                // Сценарий УСТАНОВКИ (KN не активна -> жмем KN_DK)
                if (_dkNode != null)
                {
                    PressButton(_dkNode);
                }
            }
        }

        // Вспомогательный метод для "нажал-отпустил"
        private void PressButton(Node btnNode)
        {
            _engine.InjectChange(btnNode, true);

            // Авто-отпускание через 500мс
            System.Threading.Tasks.Task.Delay(1000).ContinueWith(_ =>
            {
                _engine.InjectChange(btnNode, false);
            });
        }

        public override void BindToLogic(Context ctx, SimulationEngine engine) { }
        protected override void OnLogicChanged() { RaisePropertyChanged(nameof(ButtonColor)); }
    }
}