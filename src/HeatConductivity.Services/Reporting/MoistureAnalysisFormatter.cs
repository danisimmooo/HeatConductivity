using HeatConductivity.Core.Calculations;
using HeatConductivity.Core.Models;

namespace HeatConductivity.Services.Reporting;

/// <summary>Текст блока «Анализ влажностного режима и точки росы» — разбор по слоям.</summary>
public static class MoistureAnalysisFormatter
{
    public static string Format(WallConstruction wall, ClimateConditions climate, IFormatProvider? culture = null)
    {
        double dewPoint = DewPointCalculator.Calculate(climate.InsideTemperature, climate.InsideHumidity);

        var text = new LineBuilder(culture);
        text.Line("=== ПОСЛОЙНЫЙ РАЗБОР ТЕМПЕРАТУР И ВЛАЖНОСТИ (НОРМЫ РБ) ===");
        text.LineF($"Т улицы = {climate.OutsideTemperature}°C | Т комнаты = {climate.InsideTemperature}°C (Влажность {climate.InsideHumidity}%)");
        text.LineF($"Расчетная точка росы: {dewPoint:F1} °C");
        text.Line();

        foreach (var layer in LayerTemperatureProfile.Calculate(wall, climate))
        {
            text.LineF($"Слой {layer.Index}: [{layer.Layer.Material.Name}] ({layer.Layer.ThicknessMm} мм)");
            text.LineF($"  • Температура в слое: от {layer.TemperatureFrom:F1} °C до {layer.TemperatureTo:F1} °C");
            text.LineF($"  • Сопротивление R слоя: {layer.Resistance:F3} м²·К/Вт");

            if (layer.ContainsDewPoint)
            {
                text.LineF($"  {TextFormat.Warning} ВНИМАНИЕ: Граница точки росы ({dewPoint:F1}°C) проходит ВНУТРИ этого слоя!");
            }

            text.Line(new string('-', 50));
        }

        return text.ToString();
    }
}
