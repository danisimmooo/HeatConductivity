using HeatConductivity.Core.Models;

namespace HeatConductivity.Core.Calculations;

/// <summary>Результат по одному слою: температуры на границах и сопротивление.</summary>
public sealed record LayerResult(
    int Index,
    Layer Layer,
    double Resistance,
    double TemperatureFrom,
    double TemperatureTo,
    bool ContainsDewPoint);

/// <summary>
/// Послойное распределение температуры (стационарное, по аналитическому R).
/// Повторяет логику вкладки «Аналитика» исходной программы: перепад ΔT делится
/// пропорционально R слоя к полному R (с учётом R_si и R_se).
/// </summary>
public static class LayerTemperatureProfile
{
    public static IReadOnlyList<LayerResult> Calculate(WallConstruction wall, ClimateConditions climate)
    {
        double dewPoint = DewPointCalculator.Calculate(climate.InsideTemperature, climate.InsideHumidity);
        double rTotal = ThermalResistanceCalculator.TotalResistance(wall);

        var results = new List<LayerResult>(wall.Layers.Count);
        double currentT = climate.OutsideTemperature;

        for (int i = 0; i < wall.Layers.Count; i++)
        {
            var layer = wall.Layers[i];
            double rLayer = layer.Resistance;
            double nextT = currentT + climate.TemperatureDifference * (rLayer / rTotal);

            bool crossesDewPoint =
                (currentT <= dewPoint && nextT >= dewPoint) ||
                (currentT >= dewPoint && nextT <= dewPoint);

            results.Add(new LayerResult(i + 1, layer, rLayer, currentT, nextT, crossesDewPoint));
            currentT = nextT;
        }

        return results;
    }
}
