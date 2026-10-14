using HeatConductivity.Core.Calculations;
using HeatConductivity.Core.Models;
using HeatConductivity.Services.Materials;
using HeatConductivity.Services.Presets;
using Xunit;

namespace HeatConductivity.Services.Tests;

public class RepositoryTests
{
    private const double Tolerance = 1e-6;

    // ---------- Материалы ----------

    [Fact]
    public void Materials_contain_original_library_in_original_order()
    {
        var all = new InMemoryMaterialRepository().GetAll();

        Assert.Equal(
            new[]
            {
                "Железобетон",
                "Кирпич керамический",
                "Блоки газосиликатные (D500)",
                "Минеральная вата (Фасад)",
                "Пенополистирол (XPS)",
                "Дерево (брус сосна/ель)",
                "Штукатурка защитно-отделочная",
            },
            all.Select(m => m.Name));
    }

    [Fact]
    public void Materials_keep_original_properties()
    {
        var xps = new InMemoryMaterialRepository().FindByName("Пенополистирол (XPS)");

        Assert.NotNull(xps);
        Assert.Equal(0.032, xps!.Lambda);
        Assert.Equal(35, xps.Density);
        Assert.Equal(1450, xps.HeatCapacity);
        Assert.Equal(100, xps.RecommendedThicknessMm);
    }

    [Fact]
    public void FindByName_ignores_case_and_returns_null_for_unknown()
    {
        var repo = new InMemoryMaterialRepository();

        Assert.Equal("Железобетон", repo.FindByName("железобетон")?.Name);
        Assert.Null(repo.FindByName("Неизвестный материал"));
        Assert.Null(repo.FindByName(null));
    }

    // ---------- Пресеты стен ----------

    public static TheoryData<int, string, double> WallPresetResistances => new()
    {
        { 0, "Газосиликат + Минвата + Штукатурка", 5.764164456 },
        { 1, "Кирпичная стена с утеплением XPS", 4.480098522 },
        { 2, "Панельный дом (Ж/Б + Минвата)", 3.811176471 },
        { 3, "Брус с утеплением минватой", 3.687241379 },
    };

    [Theory]
    [MemberData(nameof(WallPresetResistances))]
    public void WallPresets_resolve_to_the_same_resistance_as_in_core_tests(int index, string name, double expectedR)
    {
        var presets = new InMemoryWallPresetRepository().GetAll();
        var preset = presets[index];

        var wall = new WallConstruction(preset.CreateLayers(new InMemoryMaterialRepository()));

        Assert.Equal(name, preset.Name);
        Assert.Equal(expectedR, ThermalResistanceCalculator.TotalResistance(wall), Tolerance);
    }

    [Fact]
    public void WallPresets_has_four_templates_and_default_is_the_first()
    {
        var repo = new InMemoryWallPresetRepository();

        Assert.Equal(4, repo.GetAll().Count);
        Assert.Same(repo.GetAll()[0], repo.DefaultPreset);
    }

    [Fact]
    public void NewLayerDefault_is_mineral_wool_100_mm()
    {
        var layer = new InMemoryWallPresetRepository().NewLayerDefault;

        Assert.Equal("Минеральная вата (Фасад)", layer.MaterialName);
        Assert.Equal(100, layer.ThicknessMm);
    }

    [Fact]
    public void CreateLayers_throws_when_material_is_missing_in_library()
    {
        var preset = new WallPreset("Тест", new[] { new PresetLayer("Нет такого", 10) });

        var ex = Assert.Throws<InvalidOperationException>(() => preset.CreateLayers(new InMemoryMaterialRepository()));

        Assert.Contains("Нет такого", ex.Message);
    }

    // ---------- Климат ----------

    [Fact]
    public void ClimatePresets_match_original_cities_and_labels()
    {
        var presets = new InMemoryClimatePresetRepository().GetAll();

        Assert.Equal(
            new[] { "Минск (-24°C)", "Брест (-21°C)", "Витебск (-25°C)", "Гомель (-23°C)", "Гродно (-22°C)", "Могилев (-24°C)" },
            presets.Select(p => p.DisplayName));

        Assert.Equal(new double[] { 55, 55, 50, 55, 55, 50 }, presets.Select(p => p.InsideHumidity));
        Assert.All(presets, p => Assert.Equal(20, p.InsideTemperature));
    }

    [Fact]
    public void ClimatePreset_converts_to_conditions_and_default_is_Minsk()
    {
        var repo = new InMemoryClimatePresetRepository();

        Assert.Equal("Минск", repo.DefaultPreset.City);
        Assert.Equal(new ClimateConditions(-24, 20, 55), repo.DefaultPreset.ToConditions());
    }
}
