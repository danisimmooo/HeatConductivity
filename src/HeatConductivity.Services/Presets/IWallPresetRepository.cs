namespace HeatConductivity.Services.Presets;

public interface IWallPresetRepository
{
    IReadOnlyList<WallPreset> GetAll();

    /// <summary>Конструкция, с которой программа стартует (совпадает с первым шаблоном).</summary>
    WallPreset DefaultPreset { get; }

    /// <summary>Слой, который добавляется кнопкой «Слой».</summary>
    PresetLayer NewLayerDefault { get; }
}
