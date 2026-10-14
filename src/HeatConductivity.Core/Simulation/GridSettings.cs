namespace HeatConductivity.Core.Simulation;

/// <summary>Размеры расчётной сетки. По умолчанию — те же, что были зашиты в Form1.</summary>
/// <param name="Nx">Число узлов поперёк стены (по толщине).</param>
/// <param name="Ny">Число узлов вдоль стены (по высоте).</param>
/// <param name="Ly">Высота расчётного фрагмента, м.</param>
public sealed record GridSettings(int Nx, int Ny, double Ly)
{
    public static GridSettings Default { get; } = new(101, 51, 1.0);
}
