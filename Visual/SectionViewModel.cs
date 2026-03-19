using BPO_ex4.StationLogic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace BPO_ex4.Visuals
{
    public class SectionViewModel : VisualObjectViewModel
    {
        private Node _occupancyNode;
        private Node _pzNode;
        private Node _lzNode;
        private Node _lsNode;

        public double Width { get; set; }
        public double Height { get; set; } = 14;

        public bool IsSwitchSection { get; set; }
        public PointCollection LockPoints { get; set; } = new PointCollection();

        public Visibility RectVisibility => IsSwitchSection ? Visibility.Collapsed : Visibility.Visible;

        public Brush FillColor
        {
            get
            {
                if (_occupancyNode == null) return Brushes.Blue;

                bool isOccupied = !_occupancyNode.Value;
                bool isLz = _lzNode != null && _lzNode.Value;
                bool isLs = _lsNode != null && _lsNode.Value;

                if (isOccupied)
                {
                    if (isLz) return Brushes.Yellow;
                    return Brushes.Red;
                }

                if (!isOccupied)
                {
                    if (isLs) return Brushes.Blue;
                }

                if (_pzNode != null && _pzNode.Value) return Brushes.LightGreen;

                if (IsSwitchSection) return Brushes.Transparent;

                return Brushes.LightGray;
            }
        }

        public SectionViewModel(double x, double y, double w, string name, bool isSwSection = false)
        {
            X = x;
            Y = y;
            Width = w;
            Name = name;
            IsSwitchSection = isSwSection;
            ZIndex = 5;
        }

        public override void BindToLogicSect(Context ctx, SimulationEngine engine)
        {
            _engine = engine;

            _occupancyNode = ctx.GetAllNodes()
                .FirstOrDefault(n => n.Id.StartsWith("SECT_P") && (n.Description == Name || n.Description.Remove(0, 1) == Name));

            if (_occupancyNode != null)
            {
                _occupancyNode.Changed += _ => OnLogicChanged();
            }

            if (_occupancyNode != null && _occupancyNode.LogicSource?.Groups != null)
            {
                foreach (var group in _occupancyNode.LogicSource.Groups)
                {
                    if (group == null) continue;

                    var inputNode = group.FirstOrDefault(n => n.Id.StartsWith("SECT_IN") || n.Id.StartsWith("RELAY_KRK") || n.Id.StartsWith("SECT_EI1"));

                    if (inputNode != null)
                    {
                        _node = inputNode;
                        break;
                    }
                }
            }

            if (_node == null)
            {
                _node = ctx.GetAllNodes().FirstOrDefault(n => n.Id.StartsWith("SECT_IN") && n.Description == Name);
            }

            _pzNode = ctx.GetAllNodes().FirstOrDefault(n => n.Id.StartsWith("SECT_Pz") && n.Description == Name);
            _lzNode = ctx.GetAllNodes().FirstOrDefault(n => n.Id.StartsWith("SECT_Lz") && n.Description == Name);
            _lsNode = ctx.GetAllNodes().FirstOrDefault(n => n.Id.StartsWith("SECT_LS") && n.Description == Name);

            if (_pzNode != null) _pzNode.Changed += _ => OnLogicChanged();
            if (_lzNode != null) _lzNode.Changed += _ => OnLogicChanged();
            if (_lsNode != null) _lsNode.Changed += _ => OnLogicChanged();

            OnLogicChanged();
        }

        protected override void OnLogicChanged()
        {
            RaisePropertyChanged(nameof(FillColor));
            RaisePropertyChanged(nameof(RectVisibility));
        }

        protected override void OnLeftClick()
        {
            if (_node != null && _engine != null)
            {
                _engine.InjectChange(_node, !_node.Value);
            }
        }
    }
}