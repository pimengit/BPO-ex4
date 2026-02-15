using System;
using System.IO;
using System.Windows;
using BPO_ex4.StationLogic;
using BPO_ex4.Visuals;

namespace BPO_ex4
{
    public partial class StationViewWindow : Window
    {
        private SimulationEngine _engine;

        // !!! Конструктор теперь просит движок !!!
        public StationViewWindow(Context ctx, SimulationEngine engine)
        {
            InitializeComponent();
            _engine = engine;
            LoadScheme(ctx);
        }

        private void LoadScheme(Context ctx)
        {
            try
            {
                string xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "objects.xml");

                if (!File.Exists(xmlPath)) return;

                // Передаем _engine в загрузчик
                var objects = StationLoader.Load(xmlPath, ctx, _engine);
                foreach (var section in objects.OfType<SectionViewModel>())
                {
                    if (section.LogicNode != null)
                    {
                        _engine.InjectChange(section.LogicNode, false);
                        _engine.InjectChange(section.LogicNode, true);

                    }
                    MainCanvasControl.ItemsSource = objects;
                    Title = $"Схема станции (Объектов: {objects.Count})";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}