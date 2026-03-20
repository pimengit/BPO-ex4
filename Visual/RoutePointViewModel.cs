using BPO_ex4.StationLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace BPO_ex4.Visuals
{
    public class RoutePointViewModel : VisualObjectViewModel
    {
        // === ГЛОБАЛЬНЫЙ СПИСОК ВСЕХ КНОПОК ===

        public static List<RoutePointViewModel> AllPoints { get; } = new List<RoutePointViewModel>();

        // Прямо под RouteSections добавь это:
        public static Dictionary<int, Dictionary<string, bool>> RouteSwitchStates { get; set; } = new Dictionary<int, Dictionary<string, bool>>();
        public static int CurrentPreviewRouteId { get; set; } = 0; // Сохраняем ID текущего предпросмотра
        public int Number { get; set; }
        public double Width { get; set; } = 34;
        public double Height { get; set; } = 16;

        public HashSet<int> StartRoutes { get; set; } = new HashSet<int>();
        public HashSet<int> EndRoutes { get; set; } = new HashSet<int>();
        public IEnumerable<int> AssociatedRoutes => StartRoutes.Union(EndRoutes);

        public static Dictionary<int, List<string>> RouteSections { get; set; } = new Dictionary<int, List<string>>();
        public static List<string> CurrentPreviewSections { get; set; } = new List<string>();
        public static event Action PreviewChanged;

        public class RouteLogic
        {
            public int RouteNum;
            public Node KnNode;
            public Node KnoNode;
            public Node DkNode;
            public Node CancelDkNode;
        }
        public List<RouteLogic> RouteLogics { get; set; } = new List<RouteLogic>();

        private static RoutePointViewModel _firstPoint = null;
        private static event Action StateChanged;

        public Brush FillColor
        {
            get
            {
                if (RouteLogics.Any(r => r.KnNode != null && r.KnNode.Value)) return Brushes.Lime;
                if (_firstPoint == this) return Brushes.Yellow;
                return Brushes.LightGray;
            }
        }

        public Brush StrokeColor => (_firstPoint != null && _firstPoint != this && _firstPoint.StartRoutes.Intersect(this.EndRoutes).Any()) ? Brushes.Cyan : Brushes.Black;
        public double StrokeThickness => (_firstPoint != null && _firstPoint != this && _firstPoint.StartRoutes.Intersect(this.EndRoutes).Any()) ? 3 : 1;

        public RoutePointViewModel(double x, double y, string name, int number, string type, bool isEnd)
        {
            X = x; Y = y; Name = name; Number = number; ZIndex = 30;

            // Запоминаем кнопку для расчета расстояний!
            AllPoints.Add(this);

            StateChanged += () =>
            {
                RaisePropertyChanged(nameof(FillColor));
                RaisePropertyChanged(nameof(StrokeColor));
                RaisePropertyChanged(nameof(StrokeThickness));
            };
        }

        public override void BindToLogic(Context ctx, SimulationEngine engine)
        {
            _engine = engine;
            foreach (int routeNum in AssociatedRoutes)
            {
                var rl = new RouteLogic { RouteNum = routeNum };
                rl.KnNode = ctx.GetAllNodes().FirstOrDefault(n => n.Id == $"ROUTE_KN[{routeNum}]");
                rl.KnoNode = ctx.GetAllNodes().FirstOrDefault(n => n.Id == $"ROUTE_KNO[{routeNum}]");

                if (rl.KnNode != null && rl.KnNode.LogicSource?.Groups != null)
                {
                    foreach (var group in rl.KnNode.LogicSource.Groups)
                    {
                        if (group == null) continue;
                        rl.DkNode = group.FirstOrDefault(n => n.Id.StartsWith("ROUTE_KN_DK"));
                        if (rl.DkNode != null) break;
                    }
                    rl.KnNode.Changed += _ => RaisePropertyChanged(nameof(FillColor));
                }

                if (rl.KnoNode != null && rl.KnoNode.LogicSource?.Groups != null)
                {
                    foreach (var group in rl.KnoNode.LogicSource.Groups)
                    {
                        if (group == null) continue;
                        rl.CancelDkNode = group.FirstOrDefault(n => n.Id.StartsWith("ROUTE_KNO_DK"));
                        if (rl.CancelDkNode != null) break;
                    }
                }

                RouteLogics.Add(rl);
            }
        }

        protected override void OnLeftClick()
        {
            var activeRoute = RouteLogics.FirstOrDefault(r => r.KnNode != null && r.KnNode.Value);
            if (activeRoute != null)
            {
                if (activeRoute.CancelDkNode != null) PressButton(activeRoute.CancelDkNode);
                _firstPoint = null;
                StateChanged?.Invoke();
                return;
            }

            if (_firstPoint == null)
            {
                _firstPoint = this;
            }
            else if (_firstPoint == this)
            {
                _firstPoint = null;
            }
            else
            {
                int commonRouteNum = _firstPoint.StartRoutes.Intersect(this.EndRoutes).FirstOrDefault();
                if (commonRouteNum != 0)
                {
                    var commonRoute = RouteLogics.FirstOrDefault(r => r.RouteNum == commonRouteNum);
                    if (commonRoute != null && commonRoute.DkNode != null) PressButton(commonRoute.DkNode);
                }
                _firstPoint = null;
            }

            CurrentPreviewRouteId = 0;
            // ИСПРАВЛЕННЫЙ БАГ: Создаем НОВЫЙ список, а не стираем словарь!
            CurrentPreviewSections = new List<string>();
            PreviewChanged?.Invoke();
            StateChanged?.Invoke();
        }

        private void PressButton(Node btnNode)
        {
            _engine.InjectChange(btnNode, true);
            System.Threading.Tasks.Task.Delay(500).ContinueWith(_ =>
            {
                _engine.InjectChange(btnNode, false);
            });
        }

        // =================================================================
        // МАГНИТНАЯ МЫШЬ: Вызывается при движении курсора над станцией!
        // =================================================================
        public static void UpdatePreviewByMouse(double mouseX, double mouseY)
        {
            if (_firstPoint == null)
            {
                if (CurrentPreviewSections.Count > 0)
                {
                    CurrentPreviewRouteId = 0; // <--- ВОТ ЭТА СТРОЧКА! Сбрасываем ID маршрута
                    CurrentPreviewSections = new List<string>();
                    PreviewChanged?.Invoke();
                }
                return;
            }

            RoutePointViewModel closestTarget = null;
            double minDistance = double.MaxValue;

            foreach (var target in AllPoints)
            {
                if (target == _firstPoint) continue;

                // Проверяем: можно ли к ней проложить маршрут?
                if (_firstPoint.StartRoutes.Intersect(target.EndRoutes).Any())
                {
                    // Теорема Пифагора: считаем квадрат расстояния (X, Y)
                    double dist = Math.Pow(target.X - mouseX, 2) + Math.Pow(target.Y - mouseY, 2);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        closestTarget = target;
                    }
                }
            }

            // Если нашли ближайшую кнопку
            if (closestTarget != null)
            {
                int commonRouteNum = _firstPoint.StartRoutes.Intersect(closestTarget.EndRoutes).FirstOrDefault();
                if (commonRouteNum != 0 && RouteSections.ContainsKey(commonRouteNum))
                {
                    CurrentPreviewRouteId = commonRouteNum;
                    var newPreview = RouteSections[commonRouteNum];
                    // Обновляем секции ТОЛЬКО если маршрут сменился (чтобы не спамить перерисовками)
                    if (CurrentPreviewSections != newPreview)
                    {
                        
                        CurrentPreviewSections = newPreview;
                        PreviewChanged?.Invoke();
                    }
                }
            }
        }
    }
}