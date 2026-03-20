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
        public System.Collections.ObjectModel.ObservableCollection<BusyLineVM> BusyLines { get; set; } = new System.Collections.ObjectModel.ObservableCollection<BusyLineVM>();
        public Visibility RectVisibility => IsSwitchSection ? Visibility.Collapsed : Visibility.Visible;

        public Brush FillColor
        {
            get
            {
                // Для стрелок возвращаем прозрачность, чтобы SwitchViewModel рисовал сам себя
                if (IsSwitchSection) return Brushes.Transparent;

                // Для прямых секций: целиком красим в Аквамарин при предпросмотре
                if (RoutePointViewModel.CurrentPreviewRouteId > 0 && RoutePointViewModel.CurrentPreviewSections.Contains(Name))
                    return Brushes.Aqua;

                if (_occupancyNode == null) return Brushes.Blue;

                bool isOccupied = !_occupancyNode.Value;
                bool isLz = _lzNode != null && _lzNode.Value;
                bool isLs = _lsNode != null && _lsNode.Value;

                if (isOccupied) return isLz ? Brushes.Yellow : Brushes.Red;
                if (!isOccupied && isLs) return Brushes.Blue;
                if (_pzNode != null && _pzNode.Value) return Brushes.LightGreen;

                return Brushes.LightGray;
            }
        }

        public SectionViewModel(double x, double y, double w, string name, bool isSwSection = false)
        {
            X = x; Y = y; Width = w; Name = name; IsSwitchSection = isSwSection;

            // === ГЛАВНЫЙ ФИКС: Вытаскиваем линии предпросмотра ПОВЕРХ стрелок! ===
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
            // Рисуем линии только если маршрут в предпросмотре и еще не замкнут зеленой (Pz)
            bool isPreview = RoutePointViewModel.CurrentPreviewRouteId > 0 &&
                             RoutePointViewModel.CurrentPreviewSections.Contains(Name) &&
                             !(_pzNode != null && _pzNode.Value);

            for (int i = 0; i < BusyLines.Count; i++)
            {
                var line = BusyLines[i];
                Brush lineBrush = Brushes.Transparent; // По умолчанию линий не видно

                if (isPreview)
                {
                    if (IsLineActiveForPreview(i, RoutePointViewModel.CurrentPreviewRouteId))
                    {
                        lineBrush = Brushes.Aqua;
                    }
                }

                line.LineBrush = lineBrush;
            }
        }

        private bool IsLineActiveForPreview(int lineIndex, int routeNum)
        {
            if (!RoutePointViewModel.RouteSwitchStates.ContainsKey(routeNum)) return false;
            var requiredStates = RoutePointViewModel.RouteSwitchStates[routeNum];

            bool sw1Minus = ChildSwitchNames.Count > 0 && requiredStates.ContainsKey(ChildSwitchNames[0]) && requiredStates[ChildSwitchNames[0]];
            bool sw2Minus = ChildSwitchNames.Count > 1 && requiredStates.ContainsKey(ChildSwitchNames[1]) && requiredStates[ChildSwitchNames[1]];

            // Умная логика для одиночных стрелок и съездов
            if (ChildSwitchNames.Count == 1) // Если стрелка одиночная
            {
                if (lineIndex == 0) return !sw1Minus; // Прямо
                if (lineIndex == 1) return sw1Minus;  // Отклонение
            }
            else // Если это съезд (2 стрелки)
            {
                if (lineIndex == 0) return !sw1Minus && !sw2Minus; // Прямо по съезду
                if (lineIndex == 1) return sw1Minus;               // Отклонение 1
                if (lineIndex == 2) return sw2Minus;               // Отклонение 2
            }

            return false;
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