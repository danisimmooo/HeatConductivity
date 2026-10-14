using HeatConductivity.Core.Calculations;
using HeatConductivity.Core.Models;
using Xunit;

namespace HeatConductivity.Core.Tests;

/// <summary>
/// Фиксируют числа исходной программы (Form1) для четырёх встроенных пресетов при климате «Минск».
/// Ожидаемые значения посчитаны независимо по формулам из исходного кода.
/// </summary>
public class ThermalCalculationTests
{
    private const double Tolerance = 1e-6;

    public static TheoryData<string, double, double, double> Presets => new()
    {
        { "aerated", 5.764164456, 7.633369994, 0.515 },
        { "brick",   4.480098522, 9.821212588, 0.515 },
        { "panel",   3.811176471, 11.544991511, 0.380 },
        { "wood",    3.687241379, 11.933040307, 0.265 },
    };

    private static WallConstruction Wall(string key) => key switch
    {
        "aerated" => TestData.AeratedMineralPlaster(),
        "brick" => TestData.BrickXps(),
        "panel" => TestData.Panel(),
        "wood" => TestData.WoodMineralPlaster(),
        _ => throw new ArgumentOutOfRangeException(nameof(key))
    };

    [Theory]
    [MemberData(nameof(Presets))]
    public void TotalResistance_matches_original(string key, double expectedR, double _, double __)
    {
        Assert.Equal(expectedR, ThermalResistanceCalculator.TotalResistance(Wall(key)), Tolerance);
    }

    [Theory]
    [MemberData(nameof(Presets))]
    public void HeatFlux_matches_original(string key, double _, double expectedQ, double __)
    {
        Assert.Equal(expectedQ, ThermalResistanceCalculator.HeatFlux(Wall(key), TestData.Minsk), Tolerance);
    }

    [Theory]
    [MemberData(nameof(Presets))]
    public void Thickness_matches_original(string key, double _, double __, double expectedThicknessM)
    {
        Assert.Equal(expectedThicknessM, Wall(key).EffectiveThicknessM, 9);
    }

    [Fact]
    public void EmptyWall_uses_default_thickness_and_surface_resistance_only()
    {
        var wall = new WallConstruction(Array.Empty<Layer>());

        Assert.Equal(0.4, wall.EffectiveThicknessM);
        Assert.Equal(0.17, ThermalResistanceCalculator.TotalResistance(wall), 12);
    }

    [Theory]
    [InlineData(20, 55, 11.0)]
    [InlineData(20, 50, 10.0)]
    [InlineData(20, 100, 20.0)]
    public void DewPoint_matches_original_formula(double tIn, double humidity, double expected)
    {
        Assert.Equal(expected, DewPointCalculator.Calculate(tIn, humidity), 12);
    }

    [Fact]
    public void Requirement_boundary_is_inclusive()
    {
        Assert.True(ThermalResistanceCalculator.MeetsRequirement(3.20));
        Assert.False(ThermalResistanceCalculator.MeetsRequirement(3.1999));
    }

    [Fact]
    public void InsulationShortfall_matches_original_formula()
    {
        // (3.20 - 2.20) * 0.04 * 1000 = 40
        Assert.Equal(40.0, ThermalResistanceCalculator.InsulationShortfallMm(2.20), 9);
    }

    [Fact]
    public void LayerProfile_aerated_wall_matches_original()
    {
        var profile = LayerTemperatureProfile.Calculate(TestData.AeratedMineralPlaster(), TestData.Minsk);

        Assert.Equal(3, profile.Count);

        Assert.Equal(-24.0, profile[0].TemperatureFrom, Tolerance);
        Assert.Equal(-0.512708, profile[0].TemperatureTo, 5);
        Assert.Equal(3.076923077, profile[0].Resistance, Tolerance);
        Assert.False(profile[0].ContainsDewPoint);

        Assert.Equal(-0.512708, profile[1].TemperatureFrom, 5);
        Assert.Equal(18.570717, profile[1].TemperatureTo, 5);
        Assert.True(profile[1].ContainsDewPoint);   // точка росы 11 °C попадает в утеплитель

        Assert.Equal(18.570717, profile[2].TemperatureFrom, 5);
        Assert.Equal(18.702327, profile[2].TemperatureTo, 5);
        Assert.False(profile[2].ContainsDewPoint);
    }

    [Fact]
    public void LayerProfile_panel_wall_matches_original()
    {
        var profile = LayerTemperatureProfile.Calculate(TestData.Panel(), TestData.Minsk);

        Assert.Equal(-22.913413, profile[0].TemperatureTo, 5);
        Assert.Equal(17.494058, profile[1].TemperatureTo, 5);
        Assert.Equal(18.037351, profile[2].TemperatureTo, 5);
        Assert.True(profile[1].ContainsDewPoint);
    }

    [Fact]
    public void LayerProfile_never_reaches_room_temperature_because_surface_resistance_is_included()
    {
        // Поведение исходной программы: R_si входит в знаменатель, поэтому последний слой
        // заканчивается ниже температуры комнаты. Тест фиксирует это, а не одобряет.
        var profile = LayerTemperatureProfile.Calculate(TestData.AeratedMineralPlaster(), TestData.Minsk);

        Assert.True(profile[^1].TemperatureTo < TestData.Minsk.InsideTemperature);
    }
}
