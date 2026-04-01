using BPO_ex4.StationLogic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace BPO_ex4.Visuals
{
    public class SwitchViewModel : VisualObjectViewModel
    {
        private Node _pkNode;
        private Node _mkNode;
        private Node _inPlusNode;
        private Node _inMinusNode;
        private Node _puNode;
        private Node _muNode;

        public Brush SwitchBorderBrush => ParentSection?.SectionBorderBrush == Brushes.Red ? Brushes.Red : Brushes.Black;
        public double SwitchBorderThickness => ParentSection?.SectionBorderBrush == Brushes.Red ? 2 : 1;
        public double RectX { get; set; } = -120;
        public double RectY { get; set; } = -7;

        public bool IsMinus => _mkNode != null && _mkNode.Value;

        public PointCollection PlusPoints { get; set; } = new PointCollection();
        public PointCollection MinusPoints { get; set; } = new PointCollection();

        public ObservableCollection<LineGeometry> ControlPlusLines { get; set; } = new ObservableCollection<LineGeometry>();
        public ObservableCollection<LineGeometry> ControlMinusLines { get; set; } = new ObservableCollection<LineGeometry>();

        private SectionViewModel _parentSection;

        // --- НОВАЯ ЛОГИКА ЦВЕТОВ ПОЛИГОНОВ ---
        private Brush _plusFill = Brushes.Transparent;
        public Brush PlusFill { get => _plusFill; set { _plusFill = value; RaisePropertyChanged(nameof(PlusFill)); } }

        private Brush _minusFill = Brushes.Transparent;
        public Brush MinusFill { get => _minusFill; set { _minusFill = value; RaisePropertyChanged(nameof(MinusFill)); } }

        public void SetRouteState(Brush activeBrush, bool isMinus)
        {
            Brush baseColor = Brushes.Transparent;
            Brush routeColor = activeBrush == Brushes.Transparent ? Brushes.Transparent : activeBrush;

            PlusFill = isMinus ? baseColor : routeColor;
            MinusFill = isMinus ? routeColor : baseColor;
        }
        // -------------------------------------

        public SectionViewModel ParentSection
        {
            get => _parentSection;
            set
            {
                _parentSection = value;
                if (_parentSection != null)
                {
                    _parentSection.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == "FillColor") RaisePropertyChanged(nameof(SectionFillColor));

                        // ДОБАВИТЬ ВОТ ЭТИ 3 СТРОКИ:
                        if (e.PropertyName == "SectionBorderBrush")
                        {
                            RaisePropertyChanged(nameof(SwitchBorderBrush));
                            RaisePropertyChanged(nameof(SwitchBorderThickness));
                        }
                    };
                }
            }
        }

        public Brush SectionFillColor => ParentSection?.FillColor == Brushes.Transparent ? Brushes.LightGray : ParentSection?.FillColor ?? Brushes.LightGray;

        public Brush ControlPlusBrush
        {
            get
            {
                if (_pkNode == null || _mkNode == null) return Brushes.Lime;
                bool pk = _pkNode.Value, mk = _mkNode.Value;

                if (pk && !mk) return Brushes.Lime;
                if (!pk && mk) return Brushes.Transparent;

                return Brushes.Red;
            }
        }

        public Brush ControlMinusBrush
        {
            get
            {
                if (_pkNode == null || _mkNode == null) return Brushes.Transparent;
                bool pk = _pkNode.Value, mk = _mkNode.Value;

                if (!pk && mk) return Brushes.Yellow;
                if (pk && !mk) return Brushes.Transparent;

                return Brushes.Red;
            }
        }

        public SwitchViewModel(double x, double y, string name)
        {
            X = x; Y = y; Name = name; ZIndex = 10; // Выше секции, чтобы полигоны ложились поверх!
        }

        public void UpdateColor() => RaisePropertyChanged("SectionFillColor");

        private bool IsExactMatch(string description, string name)
        {
            if (string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(name)) return false;
            string safeName = Regex.Escape(name.Trim());
            string pattern = $@"(?<![\wа-яА-ЯёЁ]){safeName}(?![\wа-яА-ЯёЁ])";
            return Regex.IsMatch(description, pattern, RegexOptions.IgnoreCase);
        }

        public override void BindToLogic(Context ctx, SimulationEngine engine)
        {
            _engine = engine;

            _pkNode = ctx.GetAllNodes().FirstOrDefault(n => n.Id.StartsWith("SWITCH_PK") && IsExactMatch(n.Description, Name));
            _mkNode = ctx.GetAllNodes().FirstOrDefault(n => n.Id.StartsWith("SWITCH_MK") && IsExactMatch(n.Description, Name));

            if (_pkNode != null) _pkNode.Changed += (n) => OnLogicChanged();
            if (_mkNode != null) _mkNode.Changed += (n) => OnLogicChanged();

            _puNode = ctx.GetAllNodes().FirstOrDefault(n => Regex.IsMatch(n.Id, @"_PU\[\d+\]$") && IsExactMatch(n.Description, Name));
            _muNode = ctx.GetAllNodes().FirstOrDefault(n => Regex.IsMatch(n.Id, @"_MU\[\d+\]$") && IsExactMatch(n.Description, Name));

            if (_puNode != null) _puNode.Changed += OnAutoCommandChanged;
            if (_muNode != null) _muNode.Changed += OnAutoCommandChanged;

            var parentSwitch = ctx.GetAllNodes().FirstOrDefault(n => n.Id.StartsWith("SWITCH_PD") && IsExactMatch(n.Description, Name));

            if (parentSwitch != null && parentSwitch.LogicSource?.Groups != null)
            {
                foreach (var group in parentSwitch.LogicSource.Groups)
                {
                    if (group == null) continue;
                    foreach (var node in group)
                    {
                        if (node.Id.StartsWith("SWITCH_IN"))
                        {
                            if (_inPlusNode == null) _inPlusNode = node;
                            else if (_inMinusNode == null) _inMinusNode = node;
                        }
                    }
                }
            }

            OnLogicChanged();
        }

        private void OnAutoCommandChanged(Node commandNode)
        {
            if (commandNode.Value == true)
            {
                bool isPlusCommand = (commandNode == _puNode);
                RunSwitchSequence(isPlusCommand);
            }
        }

        private async void RunSwitchSequence(bool toPlus)
        {
            if (_engine == null || _inPlusNode == null || _inMinusNode == null) return;
            _engine.InjectChange(_inPlusNode, false);
            _engine.InjectChange(_inMinusNode, false);
            await Task.Delay(1000);
            if (toPlus) { _engine.InjectChange(_inPlusNode, true); _engine.InjectChange(_inMinusNode, false); }
            else { _engine.InjectChange(_inPlusNode, false); _engine.InjectChange(_inMinusNode, true); }
        }

        // =======================================================
        // ИЗМЕНЕННЫЙ ОБРАБОТЧИК КЛИКА: УЧИТЫВАЕМ SHIFT ДЛЯ МЕНЮ
        // =======================================================
        protected override void OnRightClick()
        {
            // Проверяем, зажат ли SHIFT
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                // Если SHIFT зажат -> открываем меню АРМ ДСП (родительской секции)
                if (ParentSection != null)
                {
                    ParentSection.IsMenuOpen = true;
                }
                return; // Не переводим стрелку!
            }

            // --- Твоя оригинальная логика перевода стрелки ---
            if (_engine == null || _inPlusNode == null || _inMinusNode == null) return;
            bool isPlus = (_pkNode != null && _pkNode.Value);
            if (isPlus) { _engine.InjectChange(_inPlusNode, false); _engine.InjectChange(_inMinusNode, true); }
            else { _engine.InjectChange(_inPlusNode, true); _engine.InjectChange(_inMinusNode, false); }
        }

        protected override void OnLeftClick()
        {
            if (_engine == null) return;
            if (_inPlusNode != null) _engine.InjectChange(_inPlusNode, false);
            if (_inMinusNode != null) _engine.InjectChange(_inMinusNode, false);
        }

        protected override void OnLogicChanged()
        {
            RaisePropertyChanged(nameof(ControlPlusBrush));
            RaisePropertyChanged(nameof(ControlMinusBrush));
            RaisePropertyChanged(nameof(IsMinus));
            ParentSection?.TriggerLogicChange();
        }
    }
}