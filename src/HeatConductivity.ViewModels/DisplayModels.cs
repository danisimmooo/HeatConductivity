namespace HeatConductivity.ViewModels;

/// <summary>Состояние расчёта динамики. View сопоставляет его с цветом подписи.</summary>
public enum CalculationStatus
{
    Ready,
    Running,
    Completed,
    Stopped,
}

/// <summary>Кадр тепловой карты: поле температур и диапазон для раскраски. Массив не изменяется после создания.</summary>
/// <param name="Temperature">Температуры узлов [i, j].</param>
/// <param name="MinTemperature">Температура, которой соответствует «холодный» цвет.</param>
/// <param name="MaxTemperature">Температура, которой соответствует «горячий» цвет.</param>
/// <param name="SensorX">Индекс узла датчика по толщине.</param>
/// <param name="SensorY">Индекс узла датчика по высоте.</param>
public sealed record HeatMapFrame(
    double[,] Temperature,
    double MinTemperature,
    double MaxTemperature,
    int SensorX,
    int SensorY);

/// <summary>Точка профиля температуры по толщине стены.</summary>
public sealed record ProfilePoint(double X, double Temperature);

/// <summary>Точка графика T(t) в месте датчика.</summary>
public sealed record SensorPoint(double Hours, double Temperature);
