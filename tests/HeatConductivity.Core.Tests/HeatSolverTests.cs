using HeatConductivity.Core.Models;
using HeatConductivity.Core.Simulation;
using Xunit;

namespace HeatConductivity.Core.Tests;

/// <summary>
/// Фиксируют поведение сетки и явной схемы из исходной Form1 (газосиликат + минвата + штукатурка,
/// климат «Минск», сетка 101×51, dt = 10 с). Ожидаемые значения получены независимой
/// реализацией того же алгоритма.
/// </summary>
public class HeatSolverTests
{
    private const double Tolerance = 1e-9;

    private static ThermalField BuildField() =>
        ThermalFieldBuilder.Build(TestData.AeratedMineralPlaster(), TestData.Minsk, GridSettings.Default);

    /// <summary>Синхронный IProgress: Progress&lt;T&gt; из BCL вызывает обработчик асинхронно.</summary>
    private sealed class SyncProgress : IProgress<SimulationProgress>
    {
        private readonly Action<SimulationProgress> _onReport;
        public SyncProgress(Action<SimulationProgress> onReport) => _onReport = onReport;
        public void Report(SimulationProgress value) => _onReport(value);
    }

    // ---------- Построение сетки ----------

    [Fact]
    public void Builder_uses_total_wall_thickness_and_default_grid()
    {
        var field = BuildField();

        Assert.Equal(101, field.Nx);
        Assert.Equal(51, field.Ny);
        Assert.Equal(0.515, field.Lx, 12);
        Assert.Equal(1.0, field.Ly);
        Assert.Equal(0.00515, field.Dx, 12);
        Assert.Equal(0.02, field.Dy, 12);
    }

    [Theory]
    [InlineData(0, 0.13)]
    [InlineData(77, 0.13)]    // последний узел газосиликата (x ≈ 0.3966 м)
    [InlineData(78, 0.040)]   // первый узел минваты (x ≈ 0.4017 м)
    [InlineData(97, 0.040)]
    [InlineData(98, 0.87)]    // штукатурка
    [InlineData(100, 0.87)]
    public void Builder_assigns_conductivity_by_layer(int i, double expectedLambda)
    {
        var field = BuildField();

        Assert.Equal(expectedLambda, field.Conductivity[i, 0], 12);
        Assert.Equal(expectedLambda, field.Conductivity[i, 25], 12);
    }

    [Fact]
    public void Builder_assigns_volumetric_heat_capacity()
    {
        var field = BuildField();

        Assert.Equal(500.0 * 840.0, field.VolumetricHeatCapacity[0, 0], 6);
        Assert.Equal(110.0 * 840.0, field.VolumetricHeatCapacity[90, 10], 6);
        Assert.Equal(1700.0 * 1050.0, field.VolumetricHeatCapacity[100, 50], 6);
    }

    [Fact]
    public void Builder_sets_initial_and_boundary_temperatures()
    {
        var field = BuildField();

        Assert.Equal(-24.0, field.Temperature[0, 0]);
        Assert.Equal(-24.0, field.Temperature[0, 25]);
        Assert.Equal(20.0, field.Temperature[100, 25]);
        Assert.Equal(20.0, field.Temperature[50, 25]);
    }

    [Fact]
    public void Builder_empty_wall_uses_default_thickness_and_fallback_material()
    {
        var empty = new WallConstruction(Array.Empty<Layer>());

        var withFallback = ThermalFieldBuilder.Build(empty, TestData.Minsk, GridSettings.Default, TestData.Concrete);
        Assert.Equal(0.4, withFallback.Lx);
        Assert.Equal(1.70, withFallback.Conductivity[50, 25]);

        var withoutFallback = ThermalFieldBuilder.Build(empty, TestData.Minsk, GridSettings.Default);
        Assert.Equal(1.0, withoutFallback.Conductivity[50, 25]);
        Assert.Equal(2_000_000.0, withoutFallback.VolumetricHeatCapacity[50, 25]);
    }

    [Fact]
    public void Field_helpers_match_original_formulas()
    {
        var field = BuildField();

        Assert.Equal(0.2575, field.XAt(50), 12);

        var profile = field.CenterlineProfile();
        Assert.Equal(101, profile.Count);
        Assert.Equal(0.0, profile[0].X);
        Assert.Equal(0.515, profile[100].X, 12);
        Assert.Equal(-24.0, profile[0].Temperature);
        Assert.Equal(20.0, profile[100].Temperature);
    }

    // ---------- Решатель ----------

    [Fact]
    public void Run_1000_steps_matches_reference_values()
    {
        var field = BuildField();

        new HeatSolver2D(field).Run(totalTimeSeconds: 10_000, dtSeconds: 10);

        var t = field.Temperature;
        Assert.Equal(-21.704154572270898, t[1, 25], Tolerance);
        Assert.Equal(-12.71437717884701, t[5, 25], Tolerance);
        Assert.Equal(11.612462017666905, t[20, 25], Tolerance);
        Assert.Equal(19.609397656457464, t[40, 25], Tolerance);
        Assert.Equal(19.952722707935216, t[50, 25], Tolerance);
        Assert.Equal(19.99998311624149, t[78, 25], Tolerance);
        Assert.Equal(19.999999989820125, t[99, 25], Tolerance);
    }

    [Fact]
    public void Run_keeps_boundary_columns_fixed()
    {
        var field = BuildField();

        new HeatSolver2D(field).Run(10_000, 10);

        for (int j = 0; j < field.Ny; j++)
        {
            Assert.Equal(-24.0, field.Temperature[0, j]);
            Assert.Equal(20.0, field.Temperature[100, j]);
        }
    }

    [Fact]
    public void Run_copies_edge_rows_from_neighbours_so_field_is_uniform_along_height()
    {
        var field = BuildField();

        new HeatSolver2D(field).Run(10_000, 10);

        var t = field.Temperature;
        Assert.Equal(t[50, 1], t[50, 0], Tolerance);        // нижняя граница
        Assert.Equal(t[50, 49], t[50, 50], Tolerance);      // верхняя граница
        Assert.Equal(19.952722707935216, t[50, 0], Tolerance);
        Assert.Equal(19.952722707935216, t[50, 50], Tolerance);
    }

    [Fact]
    public void Step_matches_Run_with_one_step()
    {
        var a = BuildField();
        var b = BuildField();

        new HeatSolver2D(a).Step(10);
        new HeatSolver2D(b).Run(10, 10);

        for (int i = 0; i < a.Nx; i++)
            for (int j = 0; j < a.Ny; j++)
                Assert.Equal(a.Temperature[i, j], b.Temperature[i, j]);
    }

    // ---------- Прогресс и отмена ----------

    [Fact]
    public void Run_reports_every_20_steps_and_at_the_end()
    {
        var field = BuildField();
        var reports = new List<SimulationProgress>();

        new HeatSolver2D(field).Run(1000, 10, new SyncProgress(reports.Add));

        Assert.Equal(new[] { 20, 40, 60, 80, 100 }, reports.Select(r => r.Step));
        Assert.All(reports, r => Assert.Equal(100, r.TotalSteps));
        Assert.Equal(200.0, reports[0].TimeSeconds, 9);
        Assert.Equal(1000.0, reports[^1].TimeSeconds, 9);
    }

    [Fact]
    public void Run_reports_final_step_even_when_not_multiple_of_20()
    {
        var field = BuildField();
        var reports = new List<SimulationProgress>();

        new HeatSolver2D(field).Run(250, 10, new SyncProgress(reports.Add));   // 25 шагов

        Assert.Equal(new[] { 20, 25 }, reports.Select(r => r.Step));
    }

    [Fact]
    public void Progress_snapshot_is_an_independent_copy()
    {
        var field = BuildField();
        SimulationProgress? first = null;

        new HeatSolver2D(field).Run(1000, 10, new SyncProgress(p => first ??= p));

        Assert.NotNull(first);
        Assert.NotSame(field.Temperature, first!.Temperature);
        // К концу расчёта поле ушло вперёд, а снимок остался на шаге 20.
        Assert.NotEqual(field.Temperature[3, 25], first.Temperature[3, 25]);
    }

    [Fact]
    public void Run_with_already_cancelled_token_does_nothing()
    {
        var field = BuildField();
        var reports = new List<SimulationProgress>();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        new HeatSolver2D(field).Run(1000, 10, new SyncProgress(reports.Add), cts.Token);

        Assert.Empty(reports);
        Assert.Equal(20.0, field.Temperature[50, 25]);
        Assert.Equal(20.0, field.Temperature[1, 25]);
    }

    [Fact]
    public void Run_stops_when_cancelled_from_progress_callback()
    {
        var field = BuildField();
        var reports = new List<SimulationProgress>();
        using var cts = new CancellationTokenSource();

        new HeatSolver2D(field).Run(10_000, 10, new SyncProgress(p =>
        {
            reports.Add(p);
            cts.Cancel();
        }), cts.Token);

        Assert.Single(reports);
        Assert.Equal(20, reports[0].Step);
    }
}
