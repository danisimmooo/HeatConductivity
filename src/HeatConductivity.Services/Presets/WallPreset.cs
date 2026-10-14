using HeatConductivity.Core.Models;
using HeatConductivity.Services.Materials;

namespace HeatConductivity.Services.Presets;

/// <summary>Слой пресета: название материала из библиотеки и толщина.</summary>
public sealed record PresetLayer(string MaterialName, double ThicknessMm);

/// <summary>Типовая конструкция стены (шаблон), первый слой — со стороны улицы.</summary>
public sealed record WallPreset(string Name, IReadOnlyList<PresetLayer> Layers)
{
    /// <summary>Превращает пресет в слои, подставляя материалы из библиотеки.</summary>
    /// <exception cref="InvalidOperationException">Материал пресета отсутствует в библиотеке.</exception>
    public IReadOnlyList<Layer> CreateLayers(IMaterialRepository materials) =>
        Layers
            .Select(l => new Layer(
                materials.FindByName(l.MaterialName)
                    ?? throw new InvalidOperationException(
                        $"Материал «{l.MaterialName}» из шаблона «{Name}» не найден в библиотеке."),
                l.ThicknessMm))
            .ToList();
}
