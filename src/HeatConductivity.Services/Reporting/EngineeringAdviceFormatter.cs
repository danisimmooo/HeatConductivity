using HeatConductivity.Core.Calculations;
using HeatConductivity.Core.Models;

namespace HeatConductivity.Services.Reporting;

/// <summary>Текст блока «Нормы СН 2.04.01-2020 (РБ)» — вывод по соответствию норме.</summary>
public static class EngineeringAdviceFormatter
{
    public static string Format(WallConstruction wall, ClimateConditions climate, IFormatProvider? culture = null)
    {
        double rTotal = ThermalResistanceCalculator.TotalResistance(wall);
        double tIn = climate.InsideTemperature;
        double humidity = climate.InsideHumidity;
        double dewPoint = DewPointCalculator.Calculate(tIn, humidity);
        double requiredR = NormativeConstants.RequiredResistance;

        var text = new LineBuilder(culture);
        text.LineF($"Толщина стены: {wall.EffectiveThicknessM * 1000:F0} мм");
        text.LineF($"Сопротивление R = {rTotal:F2} м²·К/Вт");

        if (ThermalResistanceCalculator.MeetsRequirement(rTotal))
        {
            text.LineF($"{TextFormat.Ok} Соответствует норме СН 2.04.01-2020 (РБ) (R ≥ {requiredR}).");
        }
        else
        {
            double shortfallMm = ThermalResistanceCalculator.InsulationShortfallMm(rTotal);
            text.LineF($"{TextFormat.Warning} Ниже нормы СН 2.04.01-2020 (R < {requiredR}). Увеличьте утеплитель на ~{shortfallMm:F0} мм.");
        }

        text.Line();
        text.LineF($"При Tвнутр = {tIn}°C и {humidity}% влаги роса выпадает при {dewPoint:F1}°C.");

        return text.ToString();
    }
}
