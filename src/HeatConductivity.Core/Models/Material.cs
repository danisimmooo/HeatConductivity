namespace HeatConductivity.Core.Models;

/// <summary>Строительный материал со своими теплофизическими свойствами.</summary>
/// <param name="Name">Название (используется как ключ в библиотеке материалов).</param>
/// <param name="Lambda">Теплопроводность, Вт/(м·К).</param>
/// <param name="Density">Плотность, кг/м³.</param>
/// <param name="HeatCapacity">Удельная теплоёмкость, Дж/(кг·К).</param>
/// <param name="RecommendedThicknessMm">Рекомендуемая толщина слоя по умолчанию, мм.</param>
public sealed record Material(
    string Name,
    double Lambda,
    double Density,
    double HeatCapacity,
    int RecommendedThicknessMm)
{
    /// <summary>Объёмная теплоёмкость ρ·c, Дж/(м³·К).</summary>
    public double VolumetricHeatCapacity => Density * HeatCapacity;

    public override string ToString() => Name;
}
