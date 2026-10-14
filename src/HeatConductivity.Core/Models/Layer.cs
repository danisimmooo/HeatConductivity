namespace HeatConductivity.Core.Models;

/// <summary>Слой стены: материал и толщина в миллиметрах.</summary>
public sealed record Layer(Material Material, double ThicknessMm)
{
    public double ThicknessM => ThicknessMm / 1000.0;

    /// <summary>Термическое сопротивление слоя d/λ, м²·К/Вт.</summary>
    public double Resistance => ThicknessM / Material.Lambda;
}
