using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.WindowsForms;

namespace HeatConductivity.WinForms.Controls;

/// <summary>Столбчатая диаграмма вкладов слоёв в сопротивление теплопередаче R.</summary>
internal sealed class LayerResistanceChartControl : PlotView
{
    public LayerResistanceChartControl()
    {
        Dock = DockStyle.Fill;
        Resistances = Array.Empty<double>();
    }

    public IReadOnlyList<double> Resistances
    {
        set
        {
            var model = new PlotModel { Title = "Сопротивление слоёв R (м²·К/Вт)" };

            // Слой 1 (со стороны улицы) показываем сверху.
            var categories = new CategoryAxis { Position = AxisPosition.Left, StartPosition = 1, EndPosition = 0 };
            var values = new LinearAxis { Position = AxisPosition.Bottom, Minimum = 0, MinimumPadding = 0, MaximumPadding = 0.1 };
            var bars = new BarSeries
            {
                Title = "R слоя",
                FillColor = OxyColors.DodgerBlue,
                StrokeColor = OxyColors.DarkBlue,
                StrokeThickness = 1,
            };

            for (int i = 0; i < value.Count; i++)
            {
                categories.Labels.Add($"Слой {i + 1}");
                bars.Items.Add(new BarItem { Value = value[i] });
            }

            model.Axes.Add(categories);
            model.Axes.Add(values);
            model.Series.Add(bars);
            Model = model;
        }
    }
}
