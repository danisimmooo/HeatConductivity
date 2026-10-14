namespace HeatConductivity.Services.Presets;

public interface IClimatePresetRepository
{
    IReadOnlyList<ClimatePreset> GetAll();

    /// <summary>Город, выбранный при запуске (первый в списке).</summary>
    ClimatePreset DefaultPreset { get; }
}
