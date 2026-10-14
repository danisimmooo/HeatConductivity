using HeatConductivity.Core.Models;
using HeatConductivity.ViewModels.Infrastructure;

namespace HeatConductivity.ViewModels;

/// <summary>Строка таблицы слоёв: материал и толщина.</summary>
public sealed class LayerViewModel : ObservableObject
{
    private Material _material;
    private double _thicknessMm;

    public LayerViewModel(Material material, double thicknessMm)
    {
        _material = material;
        _thicknessMm = thicknessMm;
    }

    /// <summary>
    /// Материал слоя. Смена материала пользователем подставляет его рекомендуемую толщину
    /// (как в исходной программе); материал, заданный в конструкторе, толщину не трогает.
    /// </summary>
    public Material Material
    {
        get => _material;
        set
        {
            if (SetProperty(ref _material, value))
                ThicknessMm = value.RecommendedThicknessMm;
        }
    }

    /// <summary>Толщина слоя, мм.</summary>
    public double ThicknessMm
    {
        get => _thicknessMm;
        set => SetProperty(ref _thicknessMm, value);
    }
}
