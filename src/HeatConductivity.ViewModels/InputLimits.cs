namespace HeatConductivity.ViewModels;

/// <summary>Допустимые диапазоны входных параметров (те же, что были у NumericUpDown).</summary>
public static class InputLimits
{
    public const double OutsideTemperatureMin = -50;
    public const double OutsideTemperatureMax = 50;

    public const double InsideTemperatureMin = 0;
    public const double InsideTemperatureMax = 50;

    public const double HumidityMin = 10;
    public const double HumidityMax = 100;

    public const double TotalTimeMin = 1;
    public const double TotalTimeMax = 864_000;

    public const double TimeStepMin = 1;
    public const double TimeStepMax = 600;
}
