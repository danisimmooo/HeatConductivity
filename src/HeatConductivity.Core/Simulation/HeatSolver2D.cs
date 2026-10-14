namespace HeatConductivity.Core.Simulation;

/// <summary>
/// Явная конечно-разностная схема, перенесённая из Form1 без изменений:
/// T' = T + (λ/ρc)·dt·(∂²T/∂x² + ∂²T/∂y²) во внутренних узлах; на верхней и нижней
/// границах (j = 0 и j = Ny-1) температура копируется из соседнего ряда.
/// Устойчивость схемы не проверяется — так же, как в исходной программе.
/// </summary>
public sealed class HeatSolver2D
{
    /// <summary>Как часто (в шагах) сообщать о прогрессе — как в исходной программе.</summary>
    public const int DefaultReportEveryNSteps = 20;

    private readonly ThermalField _field;
    private readonly double[,] _next;

    public HeatSolver2D(ThermalField field)
    {
        _field = field;
        _next = new double[field.Nx, field.Ny];
    }

    /// <summary>Один шаг по времени.</summary>
    public void Step(double dt)
    {
        int nx = _field.Nx, ny = _field.Ny;
        double dx = _field.Dx, dy = _field.Dy;
        var t = _field.Temperature;
        var k = _field.Conductivity;
        var c = _field.VolumetricHeatCapacity;

        for (int i = 1; i < nx - 1; i++)
        {
            for (int j = 1; j < ny - 1; j++)
            {
                double alpha = k[i, j] / c[i, j] * dt;

                double d2x = (t[i + 1, j] - 2 * t[i, j] + t[i - 1, j]) / (dx * dx);
                double d2y = (t[i, j + 1] - 2 * t[i, j] + t[i, j - 1]) / (dy * dy);

                _next[i, j] = t[i, j] + alpha * (d2x + d2y);
            }
        }

        for (int i = 1; i < nx - 1; i++)
            for (int j = 1; j < ny - 1; j++)
                t[i, j] = _next[i, j];

        for (int i = 0; i < nx; i++)
        {
            t[i, 0] = t[i, 1];
            t[i, ny - 1] = t[i, ny - 2];
        }
    }

    /// <summary>
    /// Считает от t = 0 до <paramref name="totalTimeSeconds"/> с шагом <paramref name="dtSeconds"/>.
    /// Остановка по токену — не ошибка: метод просто возвращается с тем, что успел посчитать.
    /// </summary>
    public void Run(
        double totalTimeSeconds,
        double dtSeconds,
        IProgress<SimulationProgress>? progress = null,
        CancellationToken cancellationToken = default,
        int reportEveryNSteps = DefaultReportEveryNSteps)
    {
        int totalSteps = (int)(totalTimeSeconds / dtSeconds);
        double time = 0;
        int step = 0;

        while (time < totalTimeSeconds && !cancellationToken.IsCancellationRequested)
        {
            Step(dtSeconds);
            time += dtSeconds;
            step++;

            if (progress != null && (step % reportEveryNSteps == 0 || time >= totalTimeSeconds))
            {
                var snapshot = (double[,])_field.Temperature.Clone();
                progress.Report(new SimulationProgress(step, totalSteps, time, snapshot));
            }
        }
    }
}
