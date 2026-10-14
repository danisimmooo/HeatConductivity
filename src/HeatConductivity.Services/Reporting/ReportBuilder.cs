using HeatConductivity.Core.Calculations;

namespace HeatConductivity.Services.Reporting;

/// <summary>Собирает текстовый отчёт о теплотехническом расчёте (структура и тексты — как в Form1).</summary>
public static class ReportBuilder
{
    public const string FileFilter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";

    private const string Separator = "=========================================================";

    public static string SuggestFileName(DateTime now) =>
        $"Расчет_Теплотехника_РБ_{now:yyyyMMdd_HHmm}.txt";

    public static string Build(ReportData data, IFormatProvider? culture = null)
    {
        var wall = data.Wall;
        var climate = data.Climate;

        double rTotal = ThermalResistanceCalculator.TotalResistance(wall);
        double q = ThermalResistanceCalculator.HeatFlux(wall, climate);
        double dewPoint = DewPointCalculator.Calculate(climate.InsideTemperature, climate.InsideHumidity);

        var report = new LineBuilder(culture);
        report.Line(Separator);
        report.Line("   ОТЧЁТ О ТЕПЛОТЕХНИЧЕСКОМ РАСЧЁТЕ (СН 2.04.01-2020 РБ) ");
        report.Line(Separator);
        report.LineF($"Дата проведения расчета: {data.GeneratedAt:dd.MM.yyyy HH:mm:ss}");
        report.Line();

        report.Line("1. СОСТАВ ОГРАЖДАЮЩЕЙ КОНСТРУКЦИИ:");
        int index = 1;
        foreach (var layer in wall.Layers)
        {
            report.LineF($"   Слой {index++}: {layer.Material.Name} - {layer.ThicknessMm} мм");
        }
        report.LineF($"   Общая толщина стены: {wall.EffectiveThicknessM * 1000:F0} мм");
        report.Line();

        report.Line("2. КЛИМАТИЧЕСКИЕ УСЛОВИЯ И РЕГИОН РБ:");
        report.Line($"   Населенный пункт: {data.LocationName}");
        report.LineF($"   Наружная температура (x=0): {climate.OutsideTemperature} °C");
        report.LineF($"   Внутренняя температура (x=L): {climate.InsideTemperature} °C");
        report.LineF($"   Относительная влажность внутри: {climate.InsideHumidity} %");
        report.Line();

        report.Line("3. ИНЖЕНЕРНЫЕ РАСЧЕТНЫЕ ПОКАЗАТЕЛИ:");
        report.Line($"   Сопротивление теплопередаче (R): {ResultFormatter.Resistance(rTotal, culture)}");
        report.Line("   Норматив РБ (СН 2.04.01-2020): R_req = 3.20 м²·К/Вт");
        report.Line($"   Плотность теплового потока (q): {ResultFormatter.HeatFlux(q, culture)}");
        report.Line($"   {ResultFormatter.DewPoint(dewPoint, culture)}");
        report.Line($"   Положение датчика контроля: {ResultFormatter.Sensor(data.SensorX, data.SensorTemperature, culture)}");
        report.Line();

        report.Line("4. ВЛАЖНОСТНЫЙ АНАЛИЗ И ТЕМПЕРАТУРНЫЙ ГРАДИЕНТ:");
        report.Line(MoistureAnalysisFormatter.Format(wall, climate, culture));
        report.Line();

        report.Line("5. ВЫВОДЫ И ЗАКЛЮЧЕНИЕ:");
        report.Line(EngineeringAdviceFormatter.Format(wall, climate, culture));
        report.Line(Separator);

        return report.ToString();
    }
}
