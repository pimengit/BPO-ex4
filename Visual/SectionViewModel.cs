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
        public System.Collections.Generic.List<string> ChildSwitchNames { get; set; } = new System.Collections.Generic.List<string>();

        // Список физических стрелок внутри этой секции!
        public System.Collections.Generic.List<SwitchViewModel> ChildSwitches { get; set; } = new System.Collections.Generic.List<SwitchViewModel>();
        public System.Collections.ObjectModel.ObservableCollection<BusyLineVM> BusyLines { get; set; } = new System.Collections.ObjectModel.ObservableCollection<BusyLineVM>();
        public Visibility RectVisibility => IsSwitchSection ? Visibility.Collapsed : Visibility.Visible;

        public Brush FillColor
        {
            get
            {
                if (IsSwitchSection) return Brushes.Transparent; // Фон всегда прозрачный, все рисуется линиями!

                bool isOccupied = _occupancyNode != null && !_occupancyNode.Value;
                bool isLz = _lzNode != null && _lzNode.Value;
                bool isLs = _lsNode != null && _lsNode.Value;
                bool isPz = _pzNode != null && _pzNode.Value;
                bool isPreview = RoutePointViewModel.CurrentPreviewRouteId > 0 && RoutePointViewModel.CurrentPreviewSections.Contains(Name);

                if (isPreview) return Brushes.Aqua;
                if (isOccupied) return isLz ? Brushes.Yellow : Brushes.Red;
                if (!isOccupied && isLs) return Brushes.Blue;
                if (isPz) return Brushes.Lime;

                return Brushes.LightGray;
            }
        }

        public SectionViewModel(double x, double y, double w, string name, bool isSwSection = false)
        {
            X = x; Y = y; Width = w; Name = name; IsSwitchSection = isSwSection;
            ZIndex = isSwSection ? 50 : 5;
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
            bool isPz = _pzNode != null && _pzNode.Value;
            bool isPreview = RoutePointViewModel.CurrentPreviewRouteId > 0 && RoutePointViewModel.CurrentPreviewSections.Contains(Name);

            Brush targetBrush = Brushes.Transparent;
            if (isPreview) targetBrush = Brushes.Aqua;
            if (isOccupied) targetBrush = isLz ? Brushes.Yellow : Brushes.Red;
            else if (isPz) targetBrush = Brushes.Lime;

            if (targetBrush == Brushes.Transparent)
            {
                foreach (var l in BusyLines) l.LineBrush = Brushes.Transparent;
                foreach (var sw in ChildSwitches) sw.UpdateBusyLines(Brushes.Transparent, Brushes.Transparent);
                return;
            }

            bool sw1Minus = false, sw2Minus = false;

            if (isPreview && !isPz && !isOccupied)
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

            if (ChildSwitches.Count > 0) ChildSwitches[0].UpdateBusyLines(!sw1Minus ? targetBrush : Brushes.Transparent, sw1Minus ? targetBrush : Brushes.Transparent);
            if (ChildSwitches.Count > 1) ChildSwitches[1].UpdateBusyLines(!sw2Minus ? targetBrush : Brushes.Transparent, sw2Minus ? targetBrush : Brushes.Transparent);

            var diagonals = BusyLines.Where(l => System.Math.Abs(l.StartY - l.EndY) >= 2).OrderBy(l => System.Math.Min(l.StartX, l.EndX)).ToList();

            foreach (var line in BusyLines)
            {
                line.X1 = line.StartX; line.Y1 = line.StartY; line.X2 = line.EndX; line.Y2 = line.EndY;
                Brush lineBrush = targetBrush;
                bool isStraight = System.Math.Abs(line.StartY - line.EndY) < 2;

                if (isStraight)
                {
                    if (!sw1Minus && !sw2Minus) lineBrush = targetBrush;
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
                    lineBrush = Brushes.Transparent;
                }
                line.LineBrush = lineBrush;
            }
        }

        private void ApplyCut(BusyLineVM line, BusyLineVM diag, double hy, double minX, double maxX)
        {
            double touchX = System.Math.Abs(diag.StartY - hy) < 5 ? diag.StartX : diag.EndX;
            double farX = System.Math.Abs(diag.StartY - hy) < 5 ? diag.EndX : diag.StartX;
            bool rootIsLeft = farX > touchX;
            line.X1 = touchX; line.Y1 = hy;
            if (rootIsLeft) { line.X2 = minX; line.Y2 = hy; } else { line.X2 = maxX; line.Y2 = hy; }
        }


        public void TriggerLogicChange() => OnLogicChanged();
        protected override void OnLeftClick()
        {
            if (_node != null && _engine != null) _engine.InjectChange(_node, !_node.Value);
        }
    }
}