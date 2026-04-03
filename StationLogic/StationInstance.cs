using BPO_ex4.StationLogic;
using BPO_ex4.Visuals;
using System.Collections.ObjectModel;

namespace BPO_ex4
{
    public class StationInstance
    {
        // Имя станции (например, "Северная" или "Южная")
        public string StationName { get; set; }

        // Изолированные ядро и память
        public Context Ctx { get; set; }
        public SimulationEngine Engine { get; set; }

        // Визуальные коллекции именно для ЭТОЙ станции
        public ObservableCollection<VisualObjectViewModel> VisualObjects { get; set; }
        public ObservableCollection<RouteButtonViewModel> RouteButtons { get; set; }


        public TopPanelViewModel TopPanel { get; set; }

        public StationInstance(string name)
        {
            StationName = name;
            Ctx = new Context();
            Engine = new SimulationEngine(Ctx);
            VisualObjects = new ObservableCollection<VisualObjectViewModel>();
            RouteButtons = new ObservableCollection<RouteButtonViewModel>();
            TopPanel = new TopPanelViewModel();
        }
    }
}