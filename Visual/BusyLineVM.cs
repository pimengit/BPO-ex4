using BPO_ex4.StationLogic;
using BPO_ex4.Visuals;
using System.Windows.Media;

public class BusyLineVM : VisualObjectViewModel
{
    // 1. ДОБАВЬ ВОТ ЭТИ 4 СТРОКИ:
    public double StartX { get; set; }
    public double StartY { get; set; }
    public double EndX { get; set; }
    public double EndY { get; set; }

    private double _x1;
    public double X1 { get => _x1; set { _x1 = value; RaisePropertyChanged(nameof(X1)); } }

    private double _y1;
    public double Y1 { get => _y1; set { _y1 = value; RaisePropertyChanged(nameof(Y1)); } }

    private double _x2;
    public double X2 { get => _x2; set { _x2 = value; RaisePropertyChanged(nameof(X2)); } }

    private double _y2;
    public double Y2 { get => _y2; set { _y2 = value; RaisePropertyChanged(nameof(Y2)); } }

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