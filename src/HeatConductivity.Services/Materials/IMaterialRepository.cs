using HeatConductivity.Core.Models;

namespace HeatConductivity.Services.Materials;

public interface IMaterialRepository
{
    IReadOnlyList<Material> GetAll();

    /// <summary>Поиск по названию без учёта регистра (как в исходной программе).</summary>
    Material? FindByName(string? name);
}
