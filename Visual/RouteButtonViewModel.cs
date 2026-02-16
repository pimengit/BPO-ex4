using System.Text.RegularExpressions;
using BPO_ex4.StationLogic;

namespace BPO_ex4.Visuals
{
    public class RouteButtonViewModel : VisualObjectViewModel
    {
        // Текст на кнопке (номер)
        public string ShortId { get; set; }

        // Текст описания (длинный)
        public string Description { get; set; }

        public System.Windows.Media.Brush ButtonColor =>
            (_node != null && _node.Value) ? System.Windows.Media.Brushes.Lime : System.Windows.Media.Brushes.LightGray;

        public RouteButtonViewModel(Node node, SimulationEngine engine)
        {
            _node = node;
            _engine = engine;

            Description = node.Description ?? "Нет описания";

            // Парсим номер из ID (например "ROUTE_KN_15" -> "15")
            // Ищем все цифры в конце строки или внутри скобок []
            var match = Regex.Match(node.Id, @"(\d+)(?!.*\d)");
            ShortId = match.Success ? match.Value : "?";

            // Подписка на изменение цвета
            _node.Changed += (n) => RaisePropertyChanged(nameof(ButtonColor));
        }

        protected override void OnLeftClick()
        {
            if (_node != null && _engine != null)
            {
                // Нажали - записали 1 (импульс обычно не нужен, если это маршрут)
                _engine.InjectChange(_node, true);

                // Если нужно, чтобы кнопка "отжималась" визуально через полсекунды:
                System.Threading.Tasks.Task.Delay(500).ContinueWith(_ =>
                {
                    _engine.InjectChange(_node, false);
                });
            }
        }

        // Заглушка
        public override void BindToLogic(Context ctx, SimulationEngine engine) { }
        protected override void OnLogicChanged() { RaisePropertyChanged(nameof(ButtonColor)); }
    }
}