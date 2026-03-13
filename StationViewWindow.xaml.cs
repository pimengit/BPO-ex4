using BPO_ex4.StationLogic;
using BPO_ex4.Visuals;
using System;
using System.Collections.ObjectModel; // <--- Нужно для ObservableCollection
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace BPO_ex4
{

    public partial class StationViewWindow : Window
    {

        private SimulationEngine _engine;

        private bool _isDraggingPanel = false;
        private Point _clickPoint;
        private Thickness _startMargin;




        // 1. ОБЪЯВЛЯЕМ КОЛЛЕКЦИЮ (чтобы XAML её видел)
        public ObservableCollection<RouteButtonViewModel> RouteButtons { get; set; }
            = new ObservableCollection<RouteButtonViewModel>();

        public StationViewWindow(Context ctx, SimulationEngine engine, string xmlpath)
        {
            InitializeComponent();
            _engine = engine;

            // 2. УСТАНАВЛИВАЕМ КОНТЕКСТ ДАННЫХ
            // Это говорит окну: "Ищи переменные (RouteButtons) внутри этого же файла"
            DataContext = this;

            // 3. СНАЧАЛА ЗАГРУЖАЕМ СХЕМУ
            LoadScheme(ctx, xmlpath);

            // 4. ПОТОМ ГЕНЕРИРУЕМ КНОПКИ
            GenerateRouteButtons(ctx);
        }

        // 1. ЛОГИКА ЗУМА (МАСШТАБИРОВАНИЯ)
        // ============================================================
        private void MapScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Зумим только если зажат Ctrl (стандартный UX), 
            // иначе работает обычная прокрутка ScrollViewer.
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                // Запрещаем скролл, чтобы страница не дергалась
                e.Handled = true;

                double zoomFactor = 1.1;
                if (e.Delta < 0) zoomFactor = 1.0 / 1.1;

                // Применяем масштаб
                double newScaleX = MapScale.ScaleX * zoomFactor;
                double newScaleY = MapScale.ScaleY * zoomFactor;

                // Ограничиваем зум (от 0.2x до 5.0x)
                if (newScaleX < 0.2) newScaleX = 0.2;
                if (newScaleY < 0.2) newScaleY = 0.2;
                if (newScaleX > 5.0) newScaleX = 5.0;
                if (newScaleY > 5.0) newScaleY = 5.0;

                MapScale.ScaleX = newScaleX;
                MapScale.ScaleY = newScaleY;
            }
        }

        private void Panel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Захватываем мышь, чтобы не потерять фокус при быстром движении
            _isDraggingPanel = true;
            _clickPoint = e.GetPosition(this); // Позиция относительно всего окна
            _startMargin = DraggablePanel.Margin;
            DraggablePanel.CaptureMouse();
        }

        // Двигаем мышь
        private void Panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDraggingPanel)
            {
                var currentPoint = e.GetPosition(this);
                var offset = currentPoint - _clickPoint;

                // Меняем Margin, чтобы сдвинуть панель
                // (VerticalAlignment=Top, HorizontalAlignment=Left в XAML обязательны для этого)
                DraggablePanel.Margin = new Thickness(
                    _startMargin.Left + offset.X,
                    _startMargin.Top + offset.Y,
                    0, 0);
            }
        }

        // Отпустили мышь
        private void Panel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDraggingPanel = false;
            DraggablePanel.ReleaseMouseCapture();
        }

        // ============================================================
        // СТАНДАРТНЫЕ МЕТОДЫ (Без изменений)
        // ============================================================

        private void GenerateRouteButtons(Context ctx)
        {
            RouteButtons.Clear();

            var routeNodes = ctx.GetAllNodes()
                                .Where(n => n.Id.Contains("ROUTE_KN["))
                                .Select(n => new { Node = n, Num = GetTrailingNumber(n.Id) })
                                .OrderBy(x => x.Num)
                                .Select(x => x.Node);

            foreach (var node in routeNodes)
            {
                // !!! ПЕРЕДАЕМ CTX В КОНСТРУКТОР !!!
                var btnVm = new RouteButtonViewModel(node, _engine, ctx);
                RouteButtons.Add(btnVm);
            }

            // Локальная вспомогательная функция – возвращает последнее число в строке или int.MaxValue если не найдено
            int GetTrailingNumber(string id)
            {
                var m = System.Text.RegularExpressions.Regex.Match(id, @"(\d+)(?!.*\d)");
                if (m.Success && int.TryParse(m.Value, out var v))
                    return v;
                return int.MaxValue;
            }
        }

        // Обработка НАЖАТИЯ правой кнопки
        /*private void Light_OnRightDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Shapes.Shape shape &&
                shape.DataContext is LightViewModel vm)
            {
                // 1. Захватываем мышь (чтобы не потерять событие отпускания)
                shape.CaptureMouse();

                // 2. Включаем переменную
                vm.OnRightMouseDown();

                // 3. Блокируем всплытие (чтобы не мешало другим элементам)
                e.Handled = true;
            }
        }

        // Обработка ОТПУСКАНИЯ правой кнопки
        private void Light_OnRightUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Shapes.Shape shape &&
                shape.DataContext is LightViewModel vm)
            {
                // 1. Выключаем переменную
                vm.OnRightMouseUp();

                // 2. Освобождаем мышь
                shape.ReleaseMouseCapture();

                e.Handled = true;
            }
        }*/

        // В файле StationViewWindow.xaml.cs

        private void Light_OnRightDown(object sender, MouseButtonEventArgs e)
        {
            // Находим ViewModel светофора через DataContext того элемента, на который нажали
            if (sender is FrameworkElement fe && fe.DataContext is LightViewModel vm)
            {
                vm.OnRightMouseDown();
                // Помечаем событие как обработанное, чтобы оно не ушло дальше
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



        private async void LoadScheme(Context ctx, string xmlpath)
        {
            try
            {
                string xmlPath = xmlpath;

                if (!File.Exists(xmlPath)) return;

                // 1. Загружаем объекты (синхронно, быстро)
                var objects = StationLoader.Load(xmlPath, ctx, _engine);

                // 2. Сразу отдаем объекты на отрисовку, чтобы пользователь увидел схему
                MainCanvasControl.ItemsSource = objects;
                Title = $"Схема станции (Объектов: {objects.Count})";

                // ==============================================================
                // АНИМАЦИЯ ИНИЦИАЛИЗАЦИИ
                // ==============================================================

                // 3. Ждем 1 секунду, пока окно полностью откроется и отрисуется
                await Task.Delay(3000);




                var parentNLPOs = ctx.GetAllNodes()
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
                                _engine.InjectChange(node, true);
                            }
                        }
                    }
                }
                // Находим все нужные переменные


                // Ищем переменные искусственной разделки (ROUTE_PP)
                var ppNodes = ctx.GetAllNodes()
                                 .Where(n => n.Id.Contains("ROUTE_PP"))
                                 .ToList();

                foreach (var section in objects.OfType<SectionViewModel>())
                {
                    if (section.LogicNode != null)
                    {
                        _engine.InjectChange(section.LogicNode, false);

                        // Авто-отпускание через 500мс
                        System.Threading.Tasks.Task.Delay(200).ContinueWith(_ =>
                        {
                            _engine.InjectChange(section.LogicNode, true);
                        });
                    }
                }



                // ROUTE_PP -> Нажимаем/Активируем (True) — делаем "наоборот" от секций
                foreach (var node in ppNodes)
                {
                    _engine.InjectChange(node, true);

                }


                MainCanvasControl.ItemsSource = objects;
                Title = $"Схема станции (Объектов: {objects.Count})";


            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка отрисовки: " + ex.Message);
            }
        }
    }
}