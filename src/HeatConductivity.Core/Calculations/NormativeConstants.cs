namespace HeatConductivity.Core.Calculations;

/// <summary>Нормативные константы, которые раньше были разбросаны по Form1 числами.</summary>
public static class NormativeConstants
{
    /// <summary>Сопротивление теплоотдаче внутренней поверхности R_si, м²·К/Вт.</summary>
    public const double InternalSurfaceResistance = 0.13;

    /// <summary>Сопротивление теплоотдаче наружной поверхности R_se, м²·К/Вт.</summary>
    public const double ExternalSurfaceResistance = 0.04;

    /// <summary>Требуемое сопротивление теплопередаче стены (как в исходной программе), м²·К/Вт.</summary>
    public const double RequiredResistance = 3.20;
}
