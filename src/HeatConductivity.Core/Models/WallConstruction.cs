namespace HeatConductivity.Core.Models;

/// <summary>
/// Многослойная стена. Первый слой обращён к улице (x = 0), последний — к комнате (x = L).
/// </summary>
public sealed class WallConstruction
{
    /// <summary>Толщина, которая используется для расчётной сетки, если слоёв нет (как в исходной программе).</summary>
    public const double DefaultThicknessM = 0.4;

    public WallConstruction(IEnumerable<Layer> layers)
    {
        Layers = layers.ToList();
    }

    public IReadOnlyList<Layer> Layers { get; }

    /// <summary>Сумма толщин слоёв, м (0 для пустой стены).</summary>
    public double TotalThicknessM => Layers.Sum(l => l.ThicknessM);

    /// <summary>Толщина для расчётной сетки: суммарная или <see cref="DefaultThicknessM"/>, если стена пуста.</summary>
    public double EffectiveThicknessM => TotalThicknessM > 0 ? TotalThicknessM : DefaultThicknessM;
}
