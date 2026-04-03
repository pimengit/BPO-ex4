using BPO_ex4.StationLogic;
using BPO_ex4.Visuals;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace BPO_ex4
{
    public partial class StationViewWindow : Window
    {
        // Наша главная "коробка" со всеми данными станции
        public StationInstance Station { get; private set; }

        private bool _isDraggingPanel = false;
        private Point _clickPoint;
        private Thickness _startMargin;

        public StationViewWindow(StationInstance station, string xmlpath)
        {
            InitializeComponent();
            Station = station;

            // XAML берет все данные из Station
            DataContext = Station;

            // Привязываем панель
            Station.TopPanel.BindToLogic(Station.Ctx, Station.Engine);

            // Грузим схему и кнопки (без передачи параметров, они уже есть в Station)
            LoadScheme(xmlpath);
            GenerateRouteButtons();
        }

        private async void LoadScheme(string xmlpath)
        {
            try
            {
                if (!File.Exists(xmlpath)) return;

                // 1. Грузим объекты, используя ядро именно этой станции
                var objects = StationLoader.Load(xmlpath, Station.Ctx, Station.Engine);

                // Очищаем и заполняем коллекцию
                Station.VisualObjects.Clear();
                foreach (var obj in objects) Station.VisualObjects.Add(obj);

                MainCanvasControl.ItemsSource = Station.VisualObjects;
                Title = $"Схема станции: {Station.StationName} (Объектов: {objects.Count})";

                // === АНИМАЦИЯ ИНИЦИАЛИЗАЦИИ ===
                await System.Threading.Tasks.Task.Delay(3000);

                var parentNLPOs = Station.Ctx.GetAllNodes()
                     .Where(n => n.Id.StartsWith("SIGNAL_NLPO"));

                foreach (var parentNLPO in parentNLPOs)
                {
                    if (parentNLPO.LogicSource?.Groups == null) continue;
                    foreach (var group in parentNLPO.LogicSource.Groups)
                    {
                        if (group == null) continue;
                        foreach (var node in group)
                        {
                            if (node.Id.StartsWith("SIGNAL_OUT_L["))
                            {
                                Station.Engine.InjectChange(node, true);
                            }
                        }
                    }
                }

                var ppNodes = Station.Ctx.GetAllNodes()
                                 .Where(n => n.Id.Contains("ROUTE_PP"))
                                 .ToList();

                foreach (var section in objects.OfType<SectionViewModel>())
                {
                    if (section.LogicNode != null)
                    {
                        Station.Engine.InjectChange(section.LogicNode, false);

                        System.Threading.Tasks.Task.Delay(200).ContinueWith(_ =>
                        {
                            Station.Engine.InjectChange(section.LogicNode, true);
                        });
                    }
                }

                foreach (var node in ppNodes)
                {
                    Station.Engine.InjectChange(node, true);
                }
            }
            catch (Exception ex)
            {
                // Выводим не только ошибку, но и строку, где она произошла
                MessageBox.Show("Ошибка отрисовки: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void GenerateRouteButtons()
        {
            Station.RouteButtons.Clear();

            var routeNodes = Station.Ctx.GetAllNodes()
                                .Where(n => n.Id.Contains("ROUTE_KN["))
                                .Select(n => new { Node = n, Num = GetTrailingNumber(n.Id) })
                                .OrderBy(x => x.Num)
                                .Select(x => x.Node);

            foreach (var node in routeNodes)
            {
                var btnVm = new RouteButtonViewModel(node, Station.Engine, Station.Ctx);
                Station.RouteButtons.Add(btnVm);
            }

            int GetTrailingNumber(string id)
            {
                var m = System.Text.RegularExpressions.Regex.Match(id, @"(\d+)(?!.*\d)");
                if (m.Success && int.TryParse(m.Value, out var v)) return v;
                return int.MaxValue;
            }
        }

        // ==========================================
        // ЗУМ, ДВИЖЕНИЕ ПАНЕЛИ И КЛИКИ (без изменений)
        // ==========================================
        private void MapScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                double zoomFactor = e.Delta < 0 ? 1.0 / 1.1 : 1.1;
                double newScaleX = Math.Clamp(MapScale.ScaleX * zoomFactor, 0.2, 5.0);
                double newScaleY = Math.Clamp(MapScale.ScaleY * zoomFactor, 0.2, 5.0);
                MapScale.ScaleX = newScaleX;
                MapScale.ScaleY = newScaleY;
            }
        }

        private void StationCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition((IInputElement)sender);
            RoutePointViewModel.UpdatePreviewByMouse(pos.X, pos.Y);
        }

        private void Panel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isDraggingPanel = true;
            _clickPoint = e.GetPosition(this);
            _startMargin = DraggablePanel.Margin;
            DraggablePanel.CaptureMouse();
        }

        private void Panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDraggingPanel)
            {
                var currentPoint = e.GetPosition(this);
                var offset = currentPoint - _clickPoint;
                DraggablePanel.Margin = new Thickness(_startMargin.Left + offset.X, _startMargin.Top + offset.Y, 0, 0);
            }
        }

        private void Panel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDraggingPanel = false;
            DraggablePanel.ReleaseMouseCapture();
        }

        private void Light_OnRightDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is LightViewModel vm)
            {
                vm.OnRightMouseDown();
                e.Handled = true;
            }
        }

        private void Light_OnRightUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is LightViewModel vm)
            {
                vm.OnRightMouseUp();
                e.Handled = true;
            }
        }
    }
}