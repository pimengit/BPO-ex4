using BPO_ex4.StationLogic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace BPO_ex4.Visuals
{
    public class GarsViewModel : VisualObjectViewModel
    {
        private Node[] _frNodes = new Node[3];
        private Node[] _pfrNodes = new Node[3];

        // --- НОВЫЕ УЗЛЫ ДЛЯ КЛИКОВ ---
        private Node _rogNode;
        private Node _oroNode;
        private Node _roMk;
        private Node _oroMk;

        public double RectW { get; set; } = 46;
        public double RectH { get; set; } = 16;

        public double LineX { get; set; } = 23;

        public double Cap1X { get; set; } = 0;
        public double Cap1W { get; set; } = 23;

        public double Cap2X { get; set; } = 24;
        public double Cap2W { get; set; } = 22;

        public int Number { get; set; }

        private string _textLeft = "-";
        public string TextLeft
        {
            get => _textLeft;
            set { _textLeft = value; RaisePropertyChanged(nameof(TextLeft)); }
        }

        private string _textRight = "-";
        public string TextRight
        {
            get => _textRight;
            set { _textRight = value; RaisePropertyChanged(nameof(TextRight)); }
        }

        // --- НОВЫЕ СВОЙСТВА ДЛЯ UI ---
        public bool IsRogActive => _rogNode?.Value == true;

        public Brush StrokeColor => IsRogActive ? Brushes.Red : Brushes.Black;
        public double StrokeThick => IsRogActive ? 2.0 : 1.0;
        public Visibility TextVisibility => IsRogActive ? Visibility.Collapsed : Visibility.Visible;

        public Brush FillColor
        {
            get
            {
                if (_frNodes[0] == null && _frNodes[1] == null && _frNodes[2] == null)
                    return Brushes.LightGray;
                return Brushes.White;
            }
        }

        public GarsViewModel(double x, double y, double w, string name, int number)
        {
            X = x - 15;
            Y = y + 5;
            Name = name;
            ZIndex = 20;
            Number = number;
        }

        public override void BindToLogic(Context ctx, SimulationEngine engine)
        {
            _engine = engine;

            for (int i = 0; i < 3; i++)
            {
                string id = $"GEN_FR{i}[{Number}]";
                _frNodes[i] = ctx.GetAllNodes().FirstOrDefault(n => n.Id == id);
                if (_frNodes[i] != null) _frNodes[i].Changed += _ => OnLogicChanged();
            }

            for (int i = 0; i < 3; i++)
            {
                string id = $"GEN_PFR{i}[{Number}]";
                _pfrNodes[i] = ctx.GetAllNodes().FirstOrDefault(n => n.Id == id);
                if (_pfrNodes[i] != null) _pfrNodes[i].Changed += _ => OnLogicChanged();
            }

            // Ищем узлы ROG и ORO (по номеру или имени)
            _rogNode = ctx.GetAllNodes().FirstOrDefault(n => n.Id == $"GEN_ROG[{Number}]" || (n.Id.StartsWith("GEN_ROG") && n.Description == Name));
            _oroNode = ctx.GetAllNodes().FirstOrDefault(n => n.Id == $"GEN_ORO[{Number}]" || (n.Id.StartsWith("GEN_ORO") && n.Description == Name));

            // Ищем курки
            _roMk = FindMkNode(ctx, _rogNode, "GEN_RO_MK");
            _oroMk = FindMkNode(ctx, _oroNode, "GEN_ORO_MK");

            if (_rogNode != null) _rogNode.Changed += _ => OnLogicChanged();

            OnLogicChanged();
        }

        private void UpdateState()
        {
            if (_frNodes[0] == null && _frNodes[1] == null && _frNodes[2] == null)
            {
                TextLeft = "?"; TextRight = "?";
                return;
            }

            int valFr = GetValue(_frNodes);
            int valPfr = GetValue(_pfrNodes);

            TextLeft = Decode(valFr);
            TextRight = Decode(valPfr);
        }

        private int GetValue(Node[] nodes)
        {
            int res = 0;
            if (nodes[0] != null && nodes[0].Value) res += 1;
            if (nodes[1] != null && nodes[1].Value) res += 2;
            if (nodes[2] != null && nodes[2].Value) res += 4;
            return res;
        }

        private string Decode(int code)
        {
            switch (code)
            {
                case 0: return "--";
                case 1: return "САО";
                case 2: return "РС";
                case 3: return "0";
                case 4: return "40";
                case 5: return "60";
                case 6: return "70";
                case 7: return "80";
                default: return "?";
            }
        }

        protected override void OnLogicChanged()
        {
            UpdateState();
            RaisePropertyChanged(nameof(FillColor));

            // Обновляем визуализацию блокировки
            RaisePropertyChanged(nameof(StrokeColor));
            RaisePropertyChanged(nameof(StrokeThick));
            RaisePropertyChanged(nameof(TextVisibility));
        }

        // --- ЛОГИКА КЛИКОВ ---
        protected override void OnLeftClick()
        {
            if (IsRogActive)
            {
                PulseNode(_oroMk, "GEN_ORO_MK");
            }
            else
            {
                PulseNode(_roMk, "GEN_RO_MK");
            }
        }

        private async void PulseNode(Node targetMkNode, string expectedName)
        {
            if (targetMkNode == null || _engine == null)
            {
                AppLogger.Log($"[GARS] ОШИБКА: Курок '{expectedName}' для {Name} не найден!");
                return;
            }

            AppLogger.Log($"[GARS] Импульс команды: {targetMkNode.Id}");
            _engine.InjectChange(targetMkNode, true);

            await Task.Delay(500);

            _engine.InjectChange(targetMkNode, false);
        }

        private Node FindMkNode(Context ctx, Node parentNode, string targetPrefix)
        {
            if (parentNode?.LogicSource?.Groups != null)
            {
                foreach (var group in parentNode.LogicSource.Groups)
                {
                    if (group == null) continue;
                    var found = group.FirstOrDefault(n => n.Id.StartsWith(targetPrefix));
                    if (found != null) return found;
                }
            }
            return ctx.GetAllNodes().FirstOrDefault(n => n.Id.StartsWith(targetPrefix) && (n.Id.EndsWith($"[{Number}]") || n.Description == Name));
        }
    }
}