using BPO_ex4.StationLogic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Xml;

namespace BPO_ex4.Visuals
{
    public class GarsViewModel : VisualObjectViewModel
    {
        // Массивы для бит (по 3 штуки)
        private Node[] _frNodes = new Node[3];
        private Node[] _pfrNodes = new Node[3];

        public double Width { get; set; }
        public double Height { get; set; } = 30; // Высота под текст

        // Текст левого столбца
        private string _textLeft = "-";
        public string TextLeft
        {
            get => _textLeft;
            set { _textLeft = value; RaisePropertyChanged(nameof(TextLeft)); }
        }

        // Текст правого столбца
        private string _textRight = "-";
        public string TextRight
        {
            get => _textRight;
            set { _textRight = value; RaisePropertyChanged(nameof(TextRight)); }
        }

        public Brush FillColor => Brushes.White; // Фон белый

        public GarsViewModel(double x, double y, double w, string name, int number)
        {
            X = x; Y = y; Width = w; Name = name; ZIndex = 20; Number = number ;
        }

        public override void BindToLogic(Context ctx, SimulationEngine engine)
        {
            _engine = engine;

            // Ищем биты FR0, FR1, FR2
            for (int i = 0; i < 3; i++)
            {
                string id = $"GEN_FR{i}[{Number}]"; // Имя переменной
                // Ищем переменную, у которой Description совпадает с именем ГАРС
                _frNodes[i] = ctx.GetAllNodes().FirstOrDefault(n => n.Id == id);
                if (_frNodes[i] != null) _frNodes[i].Changed += _ => UpdateState();
            }

            // Ищем биты PFR0, PFR1, PFR2
            for (int i = 0; i < 3; i++)
            {
                string id = $"GEN_PFR{i}[{Number}]";
                _pfrNodes[i] = ctx.GetAllNodes().FirstOrDefault(n => n.Id == id);
                if (_pfrNodes[i] != null) _pfrNodes[i].Changed += _ => UpdateState();
            }

            UpdateState();
        }

        private void UpdateState()
        {
            // Считаем число 0..7
            int valFr = GetValue(_frNodes);
            int valPfr = GetValue(_pfrNodes);

            // Обновляем текст
            TextLeft = Decode(valFr);
            TextRight = Decode(valPfr);
        }

        private int GetValue(Node[] nodes)
        {
            int res = 0;
            // Бит 0 -> +1
            if (nodes[0] != null && nodes[0].Value) res += 1;
            // Бит 1 -> +2
            if (nodes[1] != null && nodes[1].Value) res += 2;
            // Бит 2 -> +4
            if (nodes[2] != null && nodes[2].Value) res += 4;
            return res;
        }

        // === ТУТ ПРАВЬ ТЕКСТ КАК ХОЧЕШЬ ===
        private string Decode(int code)
        {
            switch (code)
            {
                case 0: return "--";  // 000
                case 1: return "САО";  // 001
                case 2: return "РС";  // 010
                case 3: return "0";  // 011
                case 4: return "40";  // 100
                case 5: return "60";   // 101
                case 6: return "70"; // 110 (Пример)
                case 7: return "80"; // 111 (Пример)
                default: return "?";
            }
        }

        protected override void OnLogicChanged() { UpdateState(); }
    }
}
