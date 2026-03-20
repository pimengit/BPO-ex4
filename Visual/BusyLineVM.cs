using BPO_ex4.StationLogic;
using System.Windows.Media;

namespace BPO_ex4.Visuals
{
    public class BusyLineVM : VisualObjectViewModel
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }

        private Brush _lineBrush = Brushes.Transparent;
        public Brush LineBrush
        {
            get => _lineBrush;
            set { _lineBrush = value; RaisePropertyChanged(nameof(LineBrush)); }
        }

        public BusyLineVM(double x1, double y1, double x2, double y2)
        {
            X1 = x1; Y1 = y1; X2 = x2; Y2 = y2;
        }

        public override void BindToLogic(Context ctx, SimulationEngine engine) { }
    }
}