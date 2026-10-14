using HeatConductivity.Core.Models;

namespace HeatConductivity.Core.Simulation;

/// <summary>Строит расчётную сетку по конструкции стены и климатическим условиям.</summary>
public static class ThermalFieldBuilder
{
    /// <summary>Значения, которые исходная программа брала, если материал не удалось определить.</summary>
    private const double FallbackConductivity = 1.0;
    private const double FallbackVolumetricHeatCapacity = 2_000_000.0;

    /// <summary>
    /// Создаёт поле: свойства материалов по слоям, начальная температура — комнатная,
    /// на границах x = 0 и x = Lx — температуры улицы и комнаты.
    /// </summary>
    /// <param name="fallbackMaterial">
    /// Материал для стены без слоёв. В исходной программе это был первый материал библиотеки;
    /// если не задан, берутся λ = 1.0 и ρ·c = 2·10⁶.
    /// </param>
    public static ThermalField Build(
        WallConstruction wall,
        ClimateConditions climate,
        GridSettings grid,
        Material? fallbackMaterial = null)
    {
        var field = new ThermalField(grid.Nx, grid.Ny, wall.EffectiveThicknessM, grid.Ly);
        double dx = field.Dx;

        for (int i = 0; i < field.Nx; i++)
        {
            double x = i * dx;
            double accumulated = 0;
            Material? material = fallbackMaterial;

            foreach (var layer in wall.Layers)
            {
                accumulated += layer.ThicknessM;
                material = layer.Material;
                if (x <= accumulated) break;
            }

            double k = material?.Lambda ?? FallbackConductivity;
            double c = material?.VolumetricHeatCapacity ?? FallbackVolumetricHeatCapacity;

            for (int j = 0; j < field.Ny; j++)
            {
                field.Conductivity[i, j] = k;
                field.VolumetricHeatCapacity[i, j] = c;
                field.Temperature[i, j] = climate.InsideTemperature;
            }
        }

        for (int j = 0; j < field.Ny; j++)
        {
            field.Temperature[0, j] = climate.OutsideTemperature;
            field.Temperature[field.Nx - 1, j] = climate.InsideTemperature;
        }

        return field;
    }
}
