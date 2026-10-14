namespace HeatConductivity.Core.Calculations;

public static class DewPointCalculator
{
    /// <summary>
    /// Точка росы по упрощённой формуле исходной программы: T − (100 − RH) / 5.
    /// Формула приближённая (применима при RH выше ~50 %), сохранена без изменений.
    /// </summary>
    public static double Calculate(double insideTemperature, double relativeHumidity) =>
        insideTemperature - (100 - relativeHumidity) / 5.0;
}
