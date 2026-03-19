using BPO_ex4.StationLogic;
using System.Linq;
using System.Windows.Media;

namespace BPO_ex4.Visuals
{
    public class GarsViewModel : VisualObjectViewModel
    {
        private Node[] _frNodes = new Node[3];
        private Node[] _pfrNodes = new Node[3];

        // --- БЕРЕМ ГЕОМЕТРИЮ ИЗ РАСПАРСЕННЫХ ПРАВИЛ ---
        public double RectW { get; set; } = 46; // Делаем пошире
        public double RectH { get; set; } = 16; // Чуть выше

        public double LineX { get; set; } = 23; // Разделитель ровно по центру (46 / 2)

        public double Cap1X { get; set; } = 0;
        public double Cap1W { get; set; } = 23; // Половина ширины для левого текста

        public double Cap2X { get; set; } = 24; // Правый текст начинается после разделителя
        public double Cap2W { get; set; } = 22; // И занимает оставшееся место

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

        public Brush FillColor
        {
            get
            {
                // Нет связи - серый, есть связь - белый (CWH из XML)
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

            // Если в основном XML задан dw/width > 0, берем его. Иначе берем из правил.
            //RectW = w > 0 ? w : GarsTemplateRules.RectW;
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
        }
    }
}