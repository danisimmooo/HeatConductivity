namespace HeatConductivity.Core.Models;

/// <summary>Расчётные условия по обе стороны стены.</summary>
/// <param name="OutsideTemperature">Температура снаружи (x = 0), °C.</param>
/// <param name="InsideTemperature">Температура в комнате (x = L), °C.</param>
/// <param name="InsideHumidity">Относительная влажность в комнате, %.</param>
public sealed record ClimateConditions(
    double OutsideTemperature,
    double InsideTemperature,
    double InsideHumidity)
{
    public double TemperatureDifference => InsideTemperature - OutsideTemperature;
}
