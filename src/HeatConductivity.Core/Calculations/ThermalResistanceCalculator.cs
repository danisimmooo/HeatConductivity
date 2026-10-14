using HeatConductivity.Core.Models;

namespace HeatConductivity.Core.Calculations;

/// <summary>Расчёт сопротивления теплопередаче и плотности теплового потока.</summary>
public static class ThermalResistanceCalculator
{
    /// <summary>Полное сопротивление теплопередаче R = R_si + Σ(d/λ) + R_se, м²·К/Вт.</summary>
    public static double TotalResistance(WallConstruction wall) =>
        NormativeConstants.InternalSurfaceResistance
        + NormativeConstants.ExternalSurfaceResistance
        + wall.Layers.Sum(l => l.Resistance);

    /// <summary>Плотность теплового потока q = |ΔT| / R, Вт/м².</summary>
    public static double HeatFlux(WallConstruction wall, ClimateConditions climate) =>
        Math.Abs(climate.TemperatureDifference) / TotalResistance(wall);

    public static bool MeetsRequirement(double totalResistance) =>
        totalResistance >= NormativeConstants.RequiredResistance;

    /// <summary>Примерная добавка утеплителя (мм), как считала исходная программа: (R_req − R) · 0.04 · 1000.</summary>
    public static double InsulationShortfallMm(double totalResistance) =>
        (NormativeConstants.RequiredResistance - totalResistance) * 0.04 * 1000;
}
