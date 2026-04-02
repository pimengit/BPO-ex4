using BPO_ex4.StationLogic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace BPO_ex4.Visuals
{
    public class SectionViewModel : VisualObjectViewModel
    {
        private Node _occupancyNode;
        private Node _pzNode;
        private Node _lzNode;
        private Node _lsNode;

        // === НОВЫЕ ПЕРЕМЕННЫЕ АРМ ДСП ===
        private Node _upnNode, _oupnNode;
        private Node _vpnNode, _pnNode;
        private Node _brcNode, _obrcNode;
        private Node _slsNode;

        private Node _upnMk, _oupnMk;
        private Node _vnDk;
        private Node _brcDk, _obrcDk;
        private Node _slsDk;

        // === КИСТИ ДЛЯ АРМ (Генерируем пунктиры) ===
        private static Brush _falseOccupiedBrush = CreatePatternBrush(Brushes.Black, Brushes.White);
        private static Brush _falseFreeBrush = CreatePatternBrush(Brushes.Black, Brushes.Red);

        private static Brush CreatePatternBrush(Brush baseColor, Brush patternColor)
        {
            var group = new DrawingGroup();
            // Базовый фон
            group.Children.Add(new GeometryDrawing(baseColor, null,
                new RectangleGeometry(new Rect(0, 0, 30, 30))));
            // Вертикальная полоса 4px из 8px — та же пропорция что и квадратик 4x4 из 8x8
            group.Children.Add(new GeometryDrawing(patternColor, null,
                new RectangleGeometry(new Rect(1, 5, 18, 18))));

            var db = new DrawingBrush
            {
                Drawing = group,
                Viewport = new Rect(0, 0, 13, 16),
                ViewportUnits = BrushMappingMode.Absolute,
                TileMode = TileMode.Tile
            };
            db.Freeze();
            return db;
        }

        /*private static Brush CreatePatternBrush(Brush baseColor, Brush patternColor)
        {
            var group = new DrawingGroup();
            // Базовый фон 4x4
            group.Children.Add(new GeometryDrawing(baseColor, null,
                new RectangleGeometry(new Rect(0, 0, 4, 4))));
            // Квадратик 2x2 по центру
            group.Children.Add(new GeometryDrawing(patternColor, null,
                new RectangleGeometry(new Rect(0, 1, 2, 2))));

            var db = new DrawingBrush
            {
                Drawing = group,
                Viewport = new Rect(0, 0, 4, 4),
                ViewportUnits = BrushMappingMode.Absolute,
                TileMode = TileMode.Tile
            };
            db.Freeze();
            return db;
        }*/

        public double Width { get; set; }
        public double Height { get; set; } = 14;

        public bool IsSwitchSection { get; set; }
        public System.Collections.Generic.List<SwitchViewModel> ChildSwitches { get; set; } = new System.Collections.Generic.List<SwitchViewModel>();
        public PointCollection LockPoints { get; set; } = new PointCollection();
        public System.Collections.Generic.List<string> ChildSwitchNames { get; set; } = new System.Collections.Generic.List<string>();
        public Visibility RectVisibility => IsSwitchSection ? Visibility.Collapsed : Visibility.Visible;

        // === СВОЙСТВА АРМ ДСП (UI) ===
        private bool _isMenuOpen;
        public bool IsMenuOpen
        {
            get => _isMenuOpen;
            set { _isMenuOpen = value; RaisePropertyChanged(nameof(IsMenuOpen)); }
        }

        public Visibility LsMenuVisibility => (_lsNode?.Value == true) ? Visibility.Visible : Visibility.Collapsed;
        public ICommand SlsCommand { get; }

        public string UpnMenuText => (_upnNode?.Value == true) ? "Убрать возможность подачи признака направления" : "Установить возможность подачи признака направления";
        public ICommand UpnCommand { get; }

        public Visibility VnVisibility => (_upnNode?.Value == true) ? Visibility.Visible : Visibility.Collapsed;
        public ICommand VnClickCommand { get; }

        public Brush VnColor
        {
            get
            {
                // Сначала проверяем, задано ли направление (наивысший приоритет)
                if (_pnNode?.Value == true)
                {
                    return Brushes.Yellow;
                }

                // Если направление не задано, но есть возможность его задать
                if (_vpnNode?.Value == true)
                {
                    return Brushes.Black;
                }

                // Если и направления нет, и возможности нет
                return Brushes.LightGray;
            }
        }

        public string BrcMenuText => (_brcNode?.Value == true) ? "Разблокировать РЦ" : "Блокировать РЦ";
        public ICommand BrcCommand { get; }

        public Brush SectionBorderBrush => (_brcNode?.Value == true) ? Brushes.Red : Brushes.Black;
        public double SectionBorderThickness => (_brcNode?.Value == true) ? 2.0 : 2.0;
        // ===================================

        public Brush FillColor
        {
            get
            {
                if (IsSwitchSection) return Brushes.Black;

                bool isOccupied = _occupancyNode != null && !_occupancyNode.Value;
                bool isLz = _lzNode != null && _lzNode.Value;
                bool isLs = _lsNode != null && _lsNode.Value;
                bool isPz = _pzNode != null && _pzNode.Value;
                bool isPreview = RoutePointViewModel.CurrentPreviewRouteId > 0 && RoutePointViewModel.CurrentPreviewSections.Contains(Name);

                if (isPreview) return Brushes.Aqua;
                if (isPz) return Brushes.Lime;

                // --- НОВЫЕ ЦВЕТА ПО СТАНДАРТАМ АРМ ДСП ---
                if (isOccupied) return isLz ? _falseOccupiedBrush : Brushes.White; // Занятость: белый, Ложная: белый пунктир
                if (isLs && !isOccupied) return _falseFreeBrush; // Ложная свободность: красный пунктир

                return Brushes.Black;
            }
        }

        public SectionViewModel(double x, double y, double w, string name, bool isSwSection = false)
        {
            X = x; Y = y; Width = w; Name = name; IsSwitchSection = isSwSection;
            ZIndex = 5; // Оставляем секцию ВНИЗУ, чтобы стрелки рисовались поверх!
            RoutePointViewModel.PreviewChanged += () => OnLogicChanged();

            // Инициализация новых команд
            SlsCommand = new SimpleCommand(() => PulseNode(_slsDk, "SECT_SLS_DK"));

            UpnCommand = new SimpleCommand(() => {
                if (_upnNode?.Value == false) PulseNode(_upnMk, "SECT_UPN_MK");
                else PulseNode(_oupnMk, "SECT_OUPN_MK");
            });

            BrcCommand = new SimpleCommand(() => {
                if (_brcNode?.Value == true) PulseNode(_obrcDk, "SECT_OBRC_DK");
                else PulseNode(_brcDk, "SECT_BRC_DK");
            });

            VnClickCommand = new SimpleCommand(() => {
                if (_vpnNode?.Value == true) PulseNode(_vnDk, "SECT_VN_DK");
            });
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
                    var inputNode = group.FirstOrDefault(n => n.Id.StartsWith("SECT_IN") || n.Id.StartsWith("RELAY_KRK") || n.Id.StartsWith("SECT_EI1") || n.Id.StartsWith("SECT_EI2"));
                    if (inputNode != null) { _node = inputNode; break; }
                }
            }

            if (_node == null) _node = ctx.GetAllNodes().FirstOrDefault(n => n.Id.StartsWith("SECT_IN") && n.Description == Name);

            _pzNode = ctx.GetAllNodes().FirstOrDefault(n => n.Id.StartsWith("SECT_Pz") && n.Description == Name);
            _lzNode = ctx.GetAllNodes().FirstOrDefault(n => n.Id.StartsWith("SECT_Lz") && n.Description == Name);
            _lsNode = ctx.GetAllNodes().FirstOrDefault(n => n.Id.StartsWith("SECT_LS") && n.Description == Name);

            // Ищем новые узлы АРМ ДСП
            _upnNode = FindMainNode(ctx, "SECT_UPN");
            _oupnNode = FindMainNode(ctx, "SECT_OUPN");
            _vpnNode = FindMainNode(ctx, "SECT_VPN");
            _pnNode = FindMainNode(ctx, "SECT_PN");
            _brcNode = FindMainNode(ctx, "SECT_BRC");
            _obrcNode = FindMainNode(ctx, "SECT_OBRC");
            _slsNode = FindMainNode(ctx, "SECT_SLS");

            // Ищем курки (Сначала во входах, потом глобально)
            _upnMk = FindMkNode(ctx, _upnNode, "SECT_UPN_MK");
            _oupnMk = FindMkNode(ctx, _oupnNode, "SECT_OUPN_MK");
            _vnDk = FindMkNode(ctx, _pnNode, "SECT_VN_DK");
            _brcDk = FindMkNode(ctx, _brcNode, "SECT_BRC_DK");
            _obrcDk = FindMkNode(ctx, _obrcNode, "SECT_OBRC_DK");
            _slsDk = FindMkNode(ctx, _slsNode, "SECT_SLS_DK");

            // Подписки
            if (_pzNode != null) _pzNode.Changed += _ => OnLogicChanged();
            if (_lzNode != null) _lzNode.Changed += _ => OnLogicChanged();
            if (_lsNode != null) _lsNode.Changed += _ => OnLogicChanged();

            // Подписываемся на новые узлы
            var armNodes = new[] { _upnNode, _vpnNode, _brcNode, _pnNode };
            foreach (var node in armNodes.Where(n => n != null))
            {
                node.Changed += (n) => OnLogicChanged();
            }

            OnLogicChanged();
        }

        protected override void OnLogicChanged()
        {
            RaisePropertyChanged(nameof(FillColor));
            RaisePropertyChanged(nameof(RectVisibility));

            // Обновляем новые свойства UI
            RaisePropertyChanged(nameof(LsMenuVisibility));
            RaisePropertyChanged(nameof(UpnMenuText));
            RaisePropertyChanged(nameof(VnVisibility));
            RaisePropertyChanged(nameof(VnColor));
            RaisePropertyChanged(nameof(BrcMenuText));
            RaisePropertyChanged(nameof(SectionBorderBrush));
            RaisePropertyChanged(nameof(SectionBorderThickness));

            if (IsSwitchSection) UpdateSwitchColors();
        }

        private void UpdateSwitchColors()
        {
            bool isOccupied = _occupancyNode != null && !_occupancyNode.Value;
            bool isLz = _lzNode != null && _lzNode.Value;
            bool isLs = _lsNode != null && _lsNode.Value;
            bool isPz = _pzNode != null && _pzNode.Value;
            bool isPreview = RoutePointViewModel.CurrentPreviewRouteId > 0 && RoutePointViewModel.CurrentPreviewSections.Contains(Name);

            Brush targetBrush = Brushes.Transparent;
            if (isPreview) targetBrush = Brushes.Aqua;
            else if (isPz) targetBrush = Brushes.Lime;

            // --- ТЕ ЖЕ ЦВЕТА ДЛЯ СТРЕЛОК ---
            else if (isOccupied) targetBrush = isLz ? _falseOccupiedBrush : Brushes.White;
            else if (isLs && !isOccupied) targetBrush = _falseFreeBrush;

            bool sw1Minus = false, sw2Minus = false;
            if (isPreview && !isPz && !isOccupied && !isLs)
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

            if (ChildSwitches.Count > 0) ChildSwitches[0].SetRouteState(targetBrush, sw1Minus);
            if (ChildSwitches.Count > 1) ChildSwitches[1].SetRouteState(targetBrush, sw2Minus);
        }

        public void TriggerLogicChange() => OnLogicChanged();

        protected override void OnLeftClick()
        {
            if (_node != null && _engine != null) _engine.InjectChange(_node, !_node.Value);
        }

        protected override async void OnRightClick()
        {
            // Ждем 50 миллисекунд, чтобы палец успел отпустить кнопку мыши (MouseUp)
            await System.Threading.Tasks.Task.Delay(50);

            IsMenuOpen = true; // Теперь открываем спокойно
        }

        // --- Вспомогательные методы ---

        private async void PulseNode(Node targetMkNode, string expectedName)
        {
            if (targetMkNode == null || _engine == null)
            {
                AppLogger.Log($"[SECTION] ОШИБКА: Курок '{expectedName}' для {Name} не найден!");
                return;
            }

            AppLogger.Log($"[SECTION] Импульс: {targetMkNode.Id}");
            _engine.InjectChange(targetMkNode, true);

            // Никаких закрытий меню, WPF сделает это сам!

            await Task.Delay(500);
            _engine.InjectChange(targetMkNode, false);
        }

        private Node FindMainNode(Context ctx, string prefix)
        {
            return ctx.GetAllNodes().FirstOrDefault(n => n.Id.StartsWith(prefix) && n.Description == Name);
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
            return ctx.GetAllNodes().FirstOrDefault(n => n.Id.StartsWith(targetPrefix) && n.Description == Name);
        }
    }
}