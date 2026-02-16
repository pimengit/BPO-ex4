using System;
using System.IO;
using System.Windows;
using System.Collections.ObjectModel; // <--- Нужно для ObservableCollection
using System.Linq;
using BPO_ex4.StationLogic;
using BPO_ex4.Visuals;

namespace BPO_ex4
{

    public partial class StationViewWindow : Window
    {

        private SimulationEngine _engine;

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

        private void GenerateRouteButtons(Context ctx)
        {
            RouteButtons.Clear();

            // Ищем переменные, в названии которых есть "ROUTE_KN"
            // ВАЖНО: Используем двойные кавычки "...", так как это строка.
            var routeNodes = ctx.GetAllNodes()
                                .Where(n => n.Id.Contains("ROUTE_KN"))
                                .OrderBy(n => n.Id);

            foreach (var node in routeNodes)
            {
                var btnVm = new RouteButtonViewModel(node, _engine);
                RouteButtons.Add(btnVm);
            }
        }


        private void LoadScheme(Context ctx, string xmlpath)
        {
            try
            {
                string xmlPath = xmlpath;

                if (!File.Exists(xmlPath)) return;

                // Передаем _engine в загрузчик
                var objects = StationLoader.Load(xmlPath, ctx, _engine);

                // Продергиваем логику секций (Reset)
                foreach (var section in objects.OfType<SectionViewModel>())
                {
                    if (section.LogicNode != null)
                    {
                        _engine.InjectChange(section.LogicNode, false);
                        _engine.InjectChange(section.LogicNode, true);
                    }
                }

                // 5. ПРИСВАИВАЕМ ИСТОЧНИК ПОСЛЕ ЦИКЛА (Оптимизация)
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