namespace HeatConductivity.Core.Simulation;

/// <summary>
/// Двумерное поле: температура, теплопроводность и объёмная теплоёмкость в узлах сетки.
/// Индексы: [i, j], где i — поперёк стены (x от 0 на улице до Lx в комнате), j — по высоте.
/// </summary>
public sealed class ThermalField
{
    public ThermalField(int nx, int ny, double lx, double ly)
    {
        Nx = nx;
        Ny = ny;
        Lx = lx;
        Ly = ly;
        Temperature = new double[nx, ny];
        Conductivity = new double[nx, ny];
        VolumetricHeatCapacity = new double[nx, ny];
    }

    public int Nx { get; }
    public int Ny { get; }

    /// <summary>Толщина расчётной области, м.</summary>
    public double Lx { get; }

    /// <summary>Высота расчётной области, м.</summary>
    public double Ly { get; }

    public double Dx => Lx / (Nx - 1);
    public double Dy => Ly / (Ny - 1);

    /// <summary>Температура, °C.</summary>
    public double[,] Temperature { get; }

    /// <summary>Теплопроводность λ, Вт/(м·К).</summary>
    public double[,] Conductivity { get; }

    /// <summary>Объёмная теплоёмкость ρ·c, Дж/(м³·К).</summary>
    public double[,] VolumetricHeatCapacity { get; }

    /// <summary>Координата x узла i, м.</summary>
    public double XAt(int i) => i / (double)(Nx - 1) * Lx;

    /// <summary>Профиль температуры по толщине вдоль средней линии (j = Ny/2).</summary>
    public IReadOnlyList<(double X, double Temperature)> CenterlineProfile()
    {
        int midY = Ny / 2;
        double dx = Dx;
        var profile = new List<(double, double)>(Nx);
        for (int i = 0; i < Nx; i++)
            profile.Add((i * dx, Temperature[i, midY]));
        return profile;
    }
}
