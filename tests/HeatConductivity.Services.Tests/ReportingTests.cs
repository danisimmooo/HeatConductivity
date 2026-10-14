using System.Globalization;
using HeatConductivity.Core.Models;
using HeatConductivity.Services.Materials;
using HeatConductivity.Services.Presets;
using HeatConductivity.Services.Reporting;
using Xunit;

namespace HeatConductivity.Services.Tests;

/// <summary>
/// Тексты и отчёт — те же строки, что выдавала Form1. Числа форматируются в InvariantCulture,
/// переводы строк нормализуются, чтобы тесты не зависели от ОС и региональных настроек.
/// </summary>
public class ReportingTests
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;
    private const string Ok = "✔";
    private const string Warn = "⚠️";

    private static readonly ClimateConditions Minsk = new(-24, 20, 55);

    private static string Normalize(string s) => s.Replace("\r\n", "\n");

    private static string Lines(params string[] lines) => string.Join("\n", lines) + "\n";

    private static WallConstruction AeratedWall()
    {
        var materials = new InMemoryMaterialRepository();
        var preset = new InMemoryWallPresetRepository().DefaultPreset;
        return new WallConstruction(preset.CreateLayers(materials));
    }

    private static WallConstruction PlasterOnlyWall()
    {
        var plaster = new InMemoryMaterialRepository().FindByName(InMemoryMaterialRepository.Plaster)!;
        return new WallConstruction(new[] { new Layer(plaster, 15) });
    }

    // ---------- Подписи ----------

    [Fact]
    public void ResultFormatter_labels_match_original()
    {
        Assert.Equal("R = 5.764 м²·К/Вт", ResultFormatter.Resistance(5.764164456, Culture));
        Assert.Equal("q = 7.6 Вт/м²", ResultFormatter.HeatFlux(7.633369994, Culture));
        Assert.Equal("Точка росы: 11.0 °C", ResultFormatter.DewPoint(11.0, Culture));
        Assert.Equal("Датчик: X = 0.26 м | T = 20.0 °C", ResultFormatter.Sensor(0.2575, 20.0, Culture));
    }

    [Fact]
    public void ResultFormatter_uses_the_requested_culture()
    {
        var ru = CultureInfo.GetCultureInfo("ru-RU");

        Assert.Equal("R = 5,764 м²·К/Вт", ResultFormatter.Resistance(5.764164456, ru));
    }

    // ---------- Вывод по норме ----------

    [Fact]
    public void Advice_for_wall_that_meets_the_norm()
    {
        var text = EngineeringAdviceFormatter.Format(AeratedWall(), Minsk, Culture);

        Assert.Equal(
            Lines(
                "Толщина стены: 515 мм",
                "Сопротивление R = 5.76 м²·К/Вт",
                $"{Ok} Соответствует норме СН 2.04.01-2020 (РБ) (R ≥ 3.2).",
                "",
                "При Tвнутр = 20°C и 55% влаги роса выпадает при 11.0°C."),
            Normalize(text));
    }

    [Fact]
    public void Advice_for_wall_below_the_norm()
    {
        var text = EngineeringAdviceFormatter.Format(PlasterOnlyWall(), Minsk, Culture);

        Assert.Equal(
            Lines(
                "Толщина стены: 15 мм",
                "Сопротивление R = 0.19 м²·К/Вт",
                $"{Warn} Ниже нормы СН 2.04.01-2020 (R < 3.2). Увеличьте утеплитель на ~121 мм.",
                "",
                "При Tвнутр = 20°C и 55% влаги роса выпадает при 11.0°C."),
            Normalize(text));
    }

    // ---------- Влажностный разбор ----------

    [Fact]
    public void MoistureAnalysis_matches_original_layer_breakdown()
    {
        var text = MoistureAnalysisFormatter.Format(AeratedWall(), Minsk, Culture);
        string dashes = new('-', 50);

        Assert.Equal(
            Lines(
                "=== ПОСЛОЙНЫЙ РАЗБОР ТЕМПЕРАТУР И ВЛАЖНОСТИ (НОРМЫ РБ) ===",
                "Т улицы = -24°C | Т комнаты = 20°C (Влажность 55%)",
                "Расчетная точка росы: 11.0 °C",
                "",
                "Слой 1: [Блоки газосиликатные (D500)] (400 мм)",
                "  • Температура в слое: от -24.0 °C до -0.5 °C",
                "  • Сопротивление R слоя: 3.077 м²·К/Вт",
                dashes,
                "Слой 2: [Минеральная вата (Фасад)] (100 мм)",
                "  • Температура в слое: от -0.5 °C до 18.6 °C",
                "  • Сопротивление R слоя: 2.500 м²·К/Вт",
                $"  {Warn} ВНИМАНИЕ: Граница точки росы (11.0°C) проходит ВНУТРИ этого слоя!",
                dashes,
                "Слой 3: [Штукатурка защитно-отделочная] (15 мм)",
                "  • Температура в слое: от 18.6 °C до 18.7 °C",
                "  • Сопротивление R слоя: 0.017 м²·К/Вт",
                dashes),
            Normalize(text));
    }

    // ---------- Отчёт ----------

    private static string BuildReport() =>
        Normalize(ReportBuilder.Build(
            new ReportData(
                new DateTime(2026, 10, 3, 15, 0, 0),
                AeratedWall(),
                "Минск (-24°C)",
                Minsk,
                SensorX: 0.2575,
                SensorTemperature: 20.0),
            Culture));

    [Fact]
    public void Report_contains_all_original_lines()
    {
        var report = BuildReport();

        Assert.Contains("   ОТЧЁТ О ТЕПЛОТЕХНИЧЕСКОМ РАСЧЁТЕ (СН 2.04.01-2020 РБ) \n", report);
        Assert.Contains("Дата проведения расчета: 03.10.2026 15:00:00\n", report);
        Assert.Contains("   Слой 1: Блоки газосиликатные (D500) - 400 мм\n", report);
        Assert.Contains("   Слой 3: Штукатурка защитно-отделочная - 15 мм\n", report);
        Assert.Contains("   Общая толщина стены: 515 мм\n", report);
        Assert.Contains("   Населенный пункт: Минск (-24°C)\n", report);
        Assert.Contains("   Наружная температура (x=0): -24 °C\n", report);
        Assert.Contains("   Внутренняя температура (x=L): 20 °C\n", report);
        Assert.Contains("   Относительная влажность внутри: 55 %\n", report);
        Assert.Contains("   Сопротивление теплопередаче (R): R = 5.764 м²·К/Вт\n", report);
        Assert.Contains("   Норматив РБ (СН 2.04.01-2020): R_req = 3.20 м²·К/Вт\n", report);
        Assert.Contains("   Плотность теплового потока (q): q = 7.6 Вт/м²\n", report);
        Assert.Contains("   Точка росы: 11.0 °C\n", report);
        Assert.Contains("   Положение датчика контроля: Датчик: X = 0.26 м | T = 20.0 °C\n", report);
    }

    [Fact]
    public void Report_has_sections_in_original_order_and_embeds_both_text_blocks()
    {
        var report = BuildReport();

        int[] positions =
        {
            report.IndexOf("1. СОСТАВ ОГРАЖДАЮЩЕЙ КОНСТРУКЦИИ:", StringComparison.Ordinal),
            report.IndexOf("2. КЛИМАТИЧЕСКИЕ УСЛОВИЯ И РЕГИОН РБ:", StringComparison.Ordinal),
            report.IndexOf("3. ИНЖЕНЕРНЫЕ РАСЧЕТНЫЕ ПОКАЗАТЕЛИ:", StringComparison.Ordinal),
            report.IndexOf("4. ВЛАЖНОСТНЫЙ АНАЛИЗ И ТЕМПЕРАТУРНЫЙ ГРАДИЕНТ:", StringComparison.Ordinal),
            report.IndexOf("=== ПОСЛОЙНЫЙ РАЗБОР ТЕМПЕРАТУР И ВЛАЖНОСТИ (НОРМЫ РБ) ===", StringComparison.Ordinal),
            report.IndexOf("5. ВЫВОДЫ И ЗАКЛЮЧЕНИЕ:", StringComparison.Ordinal),
            report.IndexOf("Толщина стены: 515 мм", StringComparison.Ordinal),
        };

        Assert.All(positions, p => Assert.True(p >= 0));
        Assert.Equal(positions.OrderBy(p => p), positions);
    }

    [Fact]
    public void Report_starts_and_ends_with_separator()
    {
        var report = BuildReport();
        string separator = new('=', 57);

        Assert.StartsWith(separator + "\n", report);
        Assert.EndsWith(separator + "\n", report);
    }

    [Fact]
    public void SuggestFileName_matches_original_pattern()
    {
        Assert.Equal(
            "Расчет_Теплотехника_РБ_20261003_1500.txt",
            ReportBuilder.SuggestFileName(new DateTime(2026, 10, 3, 15, 0, 0)));
    }
}
