namespace HeatConductivity.WinForms;

/// <summary>Общие шрифты и фабрики элементов, чтобы раскладка не повторяла одни и те же настройки.</summary>
internal static class UiStyle
{
    private const string FontFamily = "Segoe UI";

    public static Font Regular(float size = 8.5f) => new(FontFamily, size, FontStyle.Regular);

    public static Font Bold(float size = 9f) => new(FontFamily, size, FontStyle.Bold);

    public static Font Italic(float size = 8.5f) => new(FontFamily, size, FontStyle.Italic);

    public static Font Monospace(float size = 8.5f) => new("Consolas", size, FontStyle.Regular);

    public static Label CreateCaption(string text) =>
        new() { Text = text, AutoSize = true, Font = Regular() };

    /// <summary>Поле ввода числа с диапазоном и подсказкой. Привязку к модели добавляет вызывающий код.</summary>
    public static NumericUpDown CreateNumberInput(double min, double max, double value, ToolTip toolTip, string tip)
    {
        var input = new NumericUpDown
        {
            Minimum = (decimal)min,
            Maximum = (decimal)max,
            Value = (decimal)value,
            Width = 80,
            Font = Regular(),
        };
        toolTip.SetToolTip(input, tip);
        return input;
    }

    /// <summary>Привязывает значение поля ввода к числовому свойству модели в обе стороны.</summary>
    public static void BindValue(NumericUpDown input, object viewModel, string propertyName) =>
        input.DataBindings.Add(
            nameof(NumericUpDown.Value), viewModel, propertyName, true, DataSourceUpdateMode.OnPropertyChanged);
}
