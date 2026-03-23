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
        public System.Collections.Generic.List<SwitchViewModel> ChildSwitches { get; set; } = new System.Collections.Generic.List<SwitchViewModel>();
        public PointCollection LockPoints { get; set; } = new PointCollection();
        public System.Collections.Generic.List<string> ChildSwitchNames { get; set; } = new System.Collections.Generic.List<string>();
        public System.Collections.ObjectModel.ObservableCollection<BusyLineVM> BusyLines { get; set; } = new System.Collections.ObjectModel.ObservableCollection<BusyLineVM>();
        public Visibility RectVisibility => IsSwitchSection ? Visibility.Collapsed : Visibility.Visible;

        public Brush FillColor
        {
            get
            {
                if (IsSwitchSection) return Brushes.LightGray;

                bool isOccupied = _occupancyNode != null && !_occupancyNode.Value;
                bool isLz = _lzNode != null && _lzNode.Value;
                bool isLs = _lsNode != null && _lsNode.Value;
                bool isPz = _pzNode != null && _pzNode.Value;
                bool isPreview = RoutePointViewModel.CurrentPreviewRouteId > 0 && RoutePointViewModel.CurrentPreviewSections.Contains(Name);

                if (isOccupied) return isLz ? Brushes.Yellow : Brushes.Red;
                if (!isOccupied && isLs) return Brushes.Blue;
                if (isPz) return Brushes.Lime;
                if (isPreview) return Brushes.Aqua;

                return Brushes.LightGray;
            }
        }

        public SectionViewModel(double x, double y, double w, string name, bool isSwSection = false)
        {
            X = x; Y = y; Width = w; Name = name; IsSwitchSection = isSwSection;
            // ZIndex = 5 гарантирует, что соединительные линии секции будут ПОД полигоном стрелки!
            ZIndex = 5;
            RoutePointViewModel.PreviewChanged += () => OnLogicChanged();
        }

        public override void BindToLogicSect(Context ctx, SimulationEngine engine)
        {
            _engine = engine;

            _occupancyNode = ctx.GetAllNodes().FirstOrDefault(n => n.Id.StartsWith("SECT_P") && (n.Description == Name || n.Description.Remove(0, 1) == Name));
            if (_occupancyNode != null) _occupancyNode.Changed += _ => OnLogicChanged();

            if (_occupancyNode != null && _occupancyNode.LogicSource?.Groups != null)
            {
                foreach (var group in _occupancyNode.LogicSource.Groups)
                {
                    if (group == null) continue;
                    var inputNode = group.FirstOrDefault(n => n.Id.StartsWith("SECT_IN") || n.Id.StartsWith("RELAY_KRK") || n.Id.StartsWith("SECT_EI1"));
                    if (inputNode != null) { _node = inputNode; break; }
                }
            }

            if (_node == null) _node = ctx.GetAllNodes().FirstOrDefault(n => n.Id.StartsWith("SECT_IN") && n.Description == Name);

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

            if (IsSwitchSection) UpdateBusyLines();
        }

        private void UpdateBusyLines()
        {
            bool isOccupied = _occupancyNode != null && !_occupancyNode.Value;
            bool isLz = _lzNode != null && _lzNode.Value;
            bool isLs = _lsNode != null && _lsNode.Value;
            bool isPz = _pzNode != null && _pzNode.Value;
            bool isPreview = RoutePointViewModel.CurrentPreviewRouteId > 0 && RoutePointViewModel.CurrentPreviewSections.Contains(Name);

            Brush targetBrush = Brushes.Transparent;

            if (isPreview) targetBrush = Brushes.Aqua;
            else if (isPz) targetBrush = Brushes.Lime;
            else if (isOccupied) targetBrush = isLz ? Brushes.Yellow : Brushes.Red;
            else if (isLs) targetBrush = Brushes.Blue;

            bool sw1Minus = false, sw2Minus = false;

            if (isPreview)
            {
                if (RoutePointViewModel.RouteSwitchStates.TryGetValue(RoutePointViewModel.CurrentPreviewRouteId, out var reqStates))
                {
                    if (ChildSwitchNames.Count > 0 && reqStates.TryGetValue(ChildSwitchNames[0], out bool s1)) sw1Minus = s1;
                    if (ChildSwitchNames.Count > 1 && reqStates.TryGetValue(ChildSwitchNames[1], out bool s2)) sw2Minus = s2;
                }
            }
            else
            {
                if (ChildSwitches.Count > 0) sw1Minus = ChildSwitches[0].IsMinus;
                if (ChildSwitches.Count > 1) sw2Minus = ChildSwitches[1].IsMinus;
            }

            // 1. КРАСИМ САМИ ПОЛИГОНЫ СТРЕЛОК!
            if (ChildSwitches.Count > 0) ChildSwitches[0].SetRouteState(targetBrush, sw1Minus);
            if (ChildSwitches.Count > 1) ChildSwitches[1].SetRouteState(targetBrush, sw2Minus);

            // 2. Красим соединительные линии секции (остаются под полигонами благодаря ZIndex)
            if (targetBrush == Brushes.Transparent)
            {
                foreach (var l in BusyLines) l.LineBrush = Brushes.Transparent;
                return;
            }

            var diagonals = BusyLines.Where(l => System.Math.Abs(l.StartY - l.EndY) >= 2).OrderBy(l => System.Math.Min(l.StartX, l.EndX)).ToList();

            foreach (var line in BusyLines)
            {
                line.X1 = line.StartX; line.Y1 = line.StartY; line.X2 = line.EndX; line.Y2 = line.EndY;
                Brush lineBrush = targetBrush;
                bool isStraight = System.Math.Abs(line.StartY - line.EndY) < 2;

                if (isStraight)
                {
                    if (isOccupied || isLs) lineBrush = targetBrush;
                    else if (!sw1Minus && !sw2Minus) lineBrush = targetBrush;
                    else if (sw1Minus && sw2Minus) lineBrush = Brushes.Transparent;
                    else
                    {
                        double minX = System.Math.Min(line.StartX, line.EndX);
                        double maxX = System.Math.Max(line.StartX, line.EndX);
                        if (sw1Minus && diagonals.Count > 0) ApplyCut(line, diagonals[0], line.StartY, minX, maxX);
                        else if (sw2Minus && diagonals.Count > 1) ApplyCut(line, diagonals[1], line.StartY, minX, maxX);
                        else lineBrush = Brushes.Transparent;
                    }
                }
                else
                {
                    if (isOccupied || isLs) lineBrush = targetBrush;
                    else
                    {
                        int dIndex = diagonals.IndexOf(line);
                        if (dIndex == 0 && !sw1Minus) lineBrush = Brushes.Transparent;
                        if (dIndex == 1 && !sw2Minus) lineBrush = Brushes.Transparent;
                    }
                }
                line.LineBrush = lineBrush;
            }
        }

        private void ApplyCut(BusyLineVM line, BusyLineVM diag, double hy, double minX, double maxX)
        {
            bool startTouches = System.Math.Abs(diag.StartY - hy) < 5;
            double touchX = startTouches ? diag.StartX : diag.EndX;
            double farX = startTouches ? diag.EndX : diag.StartX;
            bool rootIsLeft = farX > touchX;

            line.X1 = touchX;
            line.Y1 = hy;
            line.Y2 = hy;
            if (rootIsLeft) line.X2 = minX; else line.X2 = maxX;
        }

        public void TriggerLogicChange() => OnLogicChanged();

        protected override void OnLeftClick()
        {
            if (_node != null && _engine != null)
            {
                _engine.InjectChange(_node, !_node.Value);
            }
        }
    }
}