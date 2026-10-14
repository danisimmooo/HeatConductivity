using System.Globalization;
using HeatConductivity.Core.Models;

namespace HeatConductivity.Services.Presets;

/// <summary>Климатические условия города: температуры и влажность для подстановки в форму.</summary>
public sealed record ClimatePreset(
    string City,
    double OutsideTemperature,
    double InsideTemperature,
    double InsideHumidity)
{
    /// <summary>Подпись для списка, например «Минск (-24°C)».</summary>
    public string DisplayName =>
        string.Create(CultureInfo.InvariantCulture, $"{City} ({OutsideTemperature}°C)");

    public ClimateConditions ToConditions() =>
        new(OutsideTemperature, InsideTemperature, InsideHumidity);

    public override string ToString() => DisplayName;
}
