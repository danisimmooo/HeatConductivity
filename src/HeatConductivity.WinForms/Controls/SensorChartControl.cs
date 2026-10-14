using System.Collections.ObjectModel;
using System.Collections.Specialized;
using HeatConductivity.ViewModels;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.WindowsForms;

namespace HeatConductivity.WinForms.Controls;

/// <summary>График T(t) в точке датчика: следит за коллекцией отсчётов из модели.</summary>
internal sealed class SensorChartControl : PlotView
{
    private readonly LineSeries _series = new() { Title = "T (°C)", Color = OxyColors.Red, StrokeThickness = 2 };

    public SensorChartControl()
    {
        Dock = DockStyle.Fill;
        Model = new PlotModel { Title = "Температура T(t) в месте установки датчика" };
        Model.Series.Add(_series);
    }

    public void Bind(ObservableCollection<SensorPoint> history)
    {
        Reload(history);

        history.CollectionChanged += (_, e) =>
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach (SensorPoint point in e.NewItems)
                    _series.Points.Add(new DataPoint(point.Hours, point.Temperature));

                Model.InvalidatePlot(true);
            }
            else
            {
                Reload(history);
            }
        };
    }

    private void Reload(IEnumerable<SensorPoint> history)
    {
        _series.Points.Clear();
        foreach (var point in history)
            _series.Points.Add(new DataPoint(point.Hours, point.Temperature));

        Model.InvalidatePlot(true);
    }
}
