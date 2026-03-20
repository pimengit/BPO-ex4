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
                bool isOccupied = _occupancyNode != null && !_occupancyNode.Value;
                bool isLz = _lzNode != null && _lzNode.Value;
                bool isLs = _lsNode != null && _lsNode.Value;
                bool isPz = _pzNode != null && _pzNode.Value;
                bool isPreview = RoutePointViewModel.CurrentPreviewRouteId > 0 && RoutePointViewModel.CurrentPreviewSections.Contains(Name);

                // 1. ЖЕЛЕЗНАЯ ЛОГИКА - Возвращаем родные цвета (Занятость и Зеленку) для ВСЕХ секций!
                if (isOccupied) return isLz ? Brushes.Yellow : Brushes.Red;
                if (!isOccupied && isLs) return Brushes.Blue;
                if (isPz) return Brushes.Lime; // Твоя зеленка снова в строю!

                // 2. ПРЕДПРОСМОТР (Аквамарин)
                if (isPreview)
                {
                    if (!IsSwitchSection) return Brushes.Aqua; // Обычный путь красим целиком
                    return Brushes.LightGray; // Стрелку оставляем серой, Аквамарин нарисуют линии поверх!
                }

                return Brushes.LightGray;
            }
        }

        public SectionViewModel(double x, double y, double w, string name, bool isSwSection = false)
        {
            X = x; Y = y; Width = w; Name = name; IsSwitchSection = isSwSection;
            ZIndex = isSwSection ? 50 : 5; // Вытаскиваем секцию наверх, чтобы Аквамарин лег ПОВЕРХ стрелки!
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
            bool isPreview = RoutePointViewModel.CurrentPreviewRouteId > 0 && RoutePointViewModel.CurrentPreviewSections.Contains(Name);
            bool isPz = _pzNode != null && _pzNode.Value;

            for (int i = 0; i < BusyLines.Count; i++)
            {
                var line = BusyLines[i];
                Brush lineBrush = Brushes.Transparent;

                // Рисуем Аквамарин только в предпросмотре, когда секция еще не замкнута и не занята
                if (isPreview && !isPz && !isOccupied)
                {
                    if (IsLineActiveForPreview(line, RoutePointViewModel.CurrentPreviewRouteId))
                    {
                        lineBrush = Brushes.Aqua;
                    }
                }

                line.LineBrush = lineBrush;
            }
        }

        // МАТЕМАТИКА ГЕОМЕТРИИ ЛИНИЙ
        private bool IsLineActiveForPreview(BusyLineVM line, int routeNum)
        {
            if (!RoutePointViewModel.RouteSwitchStates.ContainsKey(routeNum)) return false;
            var requiredStates = RoutePointViewModel.RouteSwitchStates[routeNum];

            // 1. По координатам понимаем, прямая это магистраль или съезд
            bool isStraightLine = System.Math.Abs(line.Y1 - line.Y2) < 2;

            // 2. Узнаем, есть ли в этой секции стрелки, которым маршрут скомандовал "В минус" (xor_mask = 1)
            var switchStates = ChildSwitchNames.Select(name => requiredStates.ContainsKey(name) && requiredStates[name]).ToList();
            bool anyMinus = switchStates.Any(s => s);

            if (isStraightLine)
            {
                // Прямую магистраль зажигаем ВСЕГДА, чтобы кусок пути до остряков не пропадал
                return true;
            }
            else
            {
                // Если линия наклонная (диагональ), сортируем все диагонали этой секции слева направо (по оси X)
                var allDiagonals = BusyLines.Where(l => System.Math.Abs(l.Y1 - l.Y2) >= 2).OrderBy(l => l.X1).ToList();
                int diagIndex = allDiagonals.IndexOf(line);

                // Если диагональ первая слева (индекс 0), она отвечает за первую стрелку. И так далее.
                if (diagIndex >= 0 && diagIndex < switchStates.Count)
                {
                    return switchStates[diagIndex];
                }

                // Подстраховка
                return anyMinus;
            }
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