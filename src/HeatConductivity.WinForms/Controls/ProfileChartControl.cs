using HeatConductivity.ViewModels;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.WindowsForms;

namespace HeatConductivity.WinForms.Controls;

/// <summary>График температуры по толщине стены T(x).</summary>
internal sealed class ProfileChartControl : PlotView
{
    private readonly LineSeries _series = new() { Title = "T(x)", Color = OxyColors.Blue, StrokeThickness = 3 };

    public ProfileChartControl()
    {
        Dock = DockStyle.Fill;
        Model = new PlotModel { Title = "Градиент температуры по слоям стены" };
        Model.Series.Add(_series);
    }

    public IReadOnlyList<ProfilePoint> Points
    {
        set
        {
            _series.Points.Clear();
            foreach (var point in value)
                _series.Points.Add(new DataPoint(point.X, point.Temperature));

            Model.InvalidatePlot(true);
        }
    }
}
