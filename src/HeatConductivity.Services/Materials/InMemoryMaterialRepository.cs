using HeatConductivity.Core.Models;

namespace HeatConductivity.Services.Materials;

/// <summary>Библиотека материалов в памяти. По умолчанию содержит набор из исходной программы.</summary>
public sealed class InMemoryMaterialRepository : IMaterialRepository
{
    public const string ReinforcedConcrete = "Железобетон";
    public const string CeramicBrick = "Кирпич керамический";
    public const string AeratedBlock = "Блоки газосиликатные (D500)";
    public const string MineralWool = "Минеральная вата (Фасад)";
    public const string Xps = "Пенополистирол (XPS)";
    public const string Wood = "Дерево (брус сосна/ель)";
    public const string Plaster = "Штукатурка защитно-отделочная";

    private readonly List<Material> _materials;

    public InMemoryMaterialRepository() : this(CreateDefaultMaterials())
    {
    }

    public InMemoryMaterialRepository(IEnumerable<Material> materials)
    {
        _materials = materials.ToList();
    }

    public IReadOnlyList<Material> GetAll() => _materials;

    public Material? FindByName(string? name) =>
        name == null
            ? null
            : _materials.Find(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public static IReadOnlyList<Material> CreateDefaultMaterials() => new[]
    {
        new Material(ReinforcedConcrete, 1.70, 2500, 840, 200),
        new Material(CeramicBrick, 0.70, 1800, 880, 250),
        new Material(AeratedBlock, 0.13, 500, 840, 400),
        new Material(MineralWool, 0.040, 110, 840, 100),
        new Material(Xps, 0.032, 35, 1450, 100),
        new Material(Wood, 0.15, 500, 2300, 150),
        new Material(Plaster, 0.87, 1700, 1050, 15),
    };
}
