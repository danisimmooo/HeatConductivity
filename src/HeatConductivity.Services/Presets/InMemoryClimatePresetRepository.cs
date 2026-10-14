namespace HeatConductivity.Services.Presets;

/// <summary>Города Беларуси из исходной программы (температура самой холодной пятидневки).</summary>
public sealed class InMemoryClimatePresetRepository : IClimatePresetRepository
{
    private static readonly IReadOnlyList<ClimatePreset> Presets = new[]
    {
        new ClimatePreset("Минск", -24, 20, 55),
        new ClimatePreset("Брест", -21, 20, 55),
        new ClimatePreset("Витебск", -25, 20, 50),
        new ClimatePreset("Гомель", -23, 20, 55),
        new ClimatePreset("Гродно", -22, 20, 55),
        new ClimatePreset("Могилев", -24, 20, 50),
    };

    public IReadOnlyList<ClimatePreset> GetAll() => Presets;

    public ClimatePreset DefaultPreset => Presets[0];
}
