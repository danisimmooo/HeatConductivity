using HeatConductivity.Services.Materials;

namespace HeatConductivity.Services.Presets;

/// <summary>Белорусские типовые стены из исходной программы.</summary>
public sealed class InMemoryWallPresetRepository : IWallPresetRepository
{
    private static readonly IReadOnlyList<WallPreset> Presets = new[]
    {
        new WallPreset("Газосиликат + Минвата + Штукатурка", new[]
        {
            new PresetLayer(InMemoryMaterialRepository.AeratedBlock, 400),
            new PresetLayer(InMemoryMaterialRepository.MineralWool, 100),
            new PresetLayer(InMemoryMaterialRepository.Plaster, 15),
        }),
        new WallPreset("Кирпичная стена с утеплением XPS", new[]
        {
            new PresetLayer(InMemoryMaterialRepository.CeramicBrick, 380),
            new PresetLayer(InMemoryMaterialRepository.Xps, 120),
            new PresetLayer(InMemoryMaterialRepository.Plaster, 15),
        }),
        new WallPreset("Панельный дом (Ж/Б + Минвата)", new[]
        {
            new PresetLayer(InMemoryMaterialRepository.ReinforcedConcrete, 160),
            new PresetLayer(InMemoryMaterialRepository.MineralWool, 140),
            new PresetLayer(InMemoryMaterialRepository.ReinforcedConcrete, 80),
        }),
        new WallPreset("Брус с утеплением минватой", new[]
        {
            new PresetLayer(InMemoryMaterialRepository.Wood, 150),
            new PresetLayer(InMemoryMaterialRepository.MineralWool, 100),
            new PresetLayer(InMemoryMaterialRepository.Plaster, 15),
        }),
    };

    public IReadOnlyList<WallPreset> GetAll() => Presets;

    public WallPreset DefaultPreset => Presets[0];

    public PresetLayer NewLayerDefault { get; } = new(InMemoryMaterialRepository.MineralWool, 100);
}
