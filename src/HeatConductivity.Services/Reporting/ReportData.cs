using HeatConductivity.Core.Models;

namespace HeatConductivity.Services.Reporting;

/// <summary>Исходные данные для отчёта. Всё остальное (R, q, тексты) отчёт считает сам.</summary>
/// <param name="GeneratedAt">Дата и время формирования.</param>
/// <param name="Wall">Конструкция стены.</param>
/// <param name="LocationName">Подпись выбранного города, например «Минск (-24°C)».</param>
/// <param name="Climate">Расчётные условия.</param>
/// <param name="SensorX">Положение датчика по толщине стены, м.</param>
/// <param name="SensorTemperature">Температура в точке датчика, °C.</param>
public sealed record ReportData(
    DateTime GeneratedAt,
    WallConstruction Wall,
    string LocationName,
    ClimateConditions Climate,
    double SensorX,
    double SensorTemperature);
