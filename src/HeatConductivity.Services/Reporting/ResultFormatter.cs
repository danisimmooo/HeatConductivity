namespace HeatConductivity.Services.Reporting;

/// <summary>
/// Короткие подписи для панели «Инженерные показатели» и отчёта.
/// Формат совпадает с подписями в исходной программе.
/// </summary>
public static class ResultFormatter
{
    public static string Resistance(double rTotal, IFormatProvider? culture = null) =>
        TextFormat.Format(culture, $"R = {rTotal:F3} м²·К/Вт");

    public static string HeatFlux(double q, IFormatProvider? culture = null) =>
        TextFormat.Format(culture, $"q = {q:F1} Вт/м²");

    public static string DewPoint(double dewPoint, IFormatProvider? culture = null) =>
        TextFormat.Format(culture, $"Точка росы: {dewPoint:F1} °C");

    public static string Sensor(double xMeters, double temperature, IFormatProvider? culture = null) =>
        TextFormat.Format(culture, $"Датчик: X = {xMeters:F2} м | T = {temperature:F1} °C");
}
