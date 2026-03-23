using BPO_ex4.StationLogic;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

        public double RectX { get; set; } = -120;
        public double RectY { get; set; } = -7;

        public bool IsMinus => _mkNode != null && _mkNode.Value;

        public PointCollection PlusPoints { get; set; } = new PointCollection();
        public PointCollection MinusPoints { get; set; } = new PointCollection();

        public ObservableCollection<LineGeometry> ControlPlusLines { get; set; } = new ObservableCollection<LineGeometry>();
        public ObservableCollection<LineGeometry> ControlMinusLines { get; set; } = new ObservableCollection<LineGeometry>();

        private SectionViewModel _parentSection;

        // Новые свойства для заливки конкретных полигонов
        private Brush _routeBrush = Brushes.LightGray;
        private bool _isRouteMinus = false;

        public Brush PlusFill => _isRouteMinus ? Brushes.LightGray : _routeBrush;
        public Brush MinusFill => _isRouteMinus ? _routeBrush : Brushes.LightGray;

        // Метод, который принимает цвет маршрута и положение стрелки от секции
        public void SetRouteState(Brush brush, bool isMinus)
        {
            _routeBrush = brush == Brushes.Transparent ? Brushes.LightGray : brush;
            _isRouteMinus = isMinus;
            RaisePropertyChanged(nameof(PlusFill));
            RaisePropertyChanged(nameof(MinusFill));
        }

        public SectionViewModel ParentSection
        {
            get => _parentSection;
            set { _parentSection = value; }
        }

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
            X = x; Y = y; Name = name; ZIndex = 10;
        }

        public void UpdateColor()
        {
            RaisePropertyChanged(nameof(PlusFill));
            RaisePropertyChanged(nameof(MinusFill));
        }

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

            AppLogger.Log($"AUTO SWITCH {Name}: Старт перевода в {(toPlus ? "ПЛЮС" : "МИНУС")}");
            _engine.InjectChange(_inPlusNode, false);
            _engine.InjectChange(_inMinusNode, false);

            await Task.Delay(1000);

            if (toPlus)
            {
                _engine.InjectChange(_inPlusNode, true);
                _engine.InjectChange(_inMinusNode, false);
            }
            else
            {
                _engine.InjectChange(_inPlusNode, false);
                _engine.InjectChange(_inMinusNode, true);
            }
            AppLogger.Log($"AUTO SWITCH {Name}: Перевод завершен");
        }

        protected override void OnRightClick()
        {
            if (_engine == null || _inPlusNode == null || _inMinusNode == null) return;
            bool isPlus = (_pkNode != null && _pkNode.Value);
            if (isPlus)
            {
                _engine.InjectChange(_inPlusNode, false);
                _engine.InjectChange(_inMinusNode, true);
            }
            else
            {
                _engine.InjectChange(_inPlusNode, true);
                _engine.InjectChange(_inMinusNode, false);
            }
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