using HeatConductivity.ViewModels;

namespace HeatConductivity.WinForms.Controls;

/// <summary>Панель «Инженерные показатели РБ»: R, q, точка росы, датчик, статус и шаги расчёта.</summary>
internal sealed class ResultsGroup : GroupBox
{
    private readonly Label _resistance = new()
    {
        Text = "R = 0.0 м²·К/Вт", AutoSize = true, Font = UiStyle.Bold(10f), ForeColor = Color.DarkBlue,
    };

    private readonly Label _heatFlux = new()
    {
        Text = "q = 0.0 Вт/м²", AutoSize = true, Font = UiStyle.Bold(10f), ForeColor = Color.DarkRed,
    };

    private readonly Label _dewPoint = new() { Text = "Точка росы: -", AutoSize = true, Font = UiStyle.Regular() };
    private readonly Label _sensor = new() { AutoSize = true, Font = UiStyle.Italic() };
    private readonly Label _status = new() { Text = "Статус: Готов", AutoSize = true, Font = UiStyle.Regular() };
    private readonly Label _steps = new() { Text = "Шагов: 0", AutoSize = true, Font = UiStyle.Regular() };

    public ResultsGroup()
    {
        Text = "Инженерные показатели РБ";
        Width = 300;
        Height = 210;
        Font = UiStyle.Bold();

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            Padding = new Padding(5),
        };
        layout.Controls.Add(_resistance, 0, 0);
        layout.Controls.Add(_heatFlux, 0, 1);
        layout.Controls.Add(_dewPoint, 0, 2);
        layout.Controls.Add(_sensor, 0, 3);
        layout.Controls.Add(_status, 0, 4);
        layout.Controls.Add(_steps, 0, 5);

        Controls.Add(layout);
    }

    public void Bind(MainViewModel vm)
    {
        BindText(_resistance, vm, nameof(MainViewModel.ResistanceText));
        BindText(_heatFlux, vm, nameof(MainViewModel.HeatFluxText));
        BindText(_dewPoint, vm, nameof(MainViewModel.DewPointText));
        BindText(_sensor, vm, nameof(MainViewModel.SensorInfoText));
        BindText(_status, vm, nameof(MainViewModel.StatusText));
        BindText(_steps, vm, nameof(MainViewModel.StepsText));

        // Точка росы выше нуля — предупреждающий цвет.
        var dewColor = new Binding(
            nameof(Label.ForeColor), vm, nameof(MainViewModel.IsDewPointAboveZero), true, DataSourceUpdateMode.Never);
        dewColor.Format += (_, e) => e.Value = e.Value is true ? Color.DarkOrange : Color.Green;
        _dewPoint.DataBindings.Add(dewColor);

        // Цвет статуса зависит от состояния расчёта.
        var statusColor = new Binding(
            nameof(Label.ForeColor), vm, nameof(MainViewModel.CalcStatus), true, DataSourceUpdateMode.Never);
        statusColor.Format += (_, e) => e.Value = e.Value is CalculationStatus status ? StatusColor(status) : SystemColors.ControlText;
        _status.DataBindings.Add(statusColor);
    }

    private static void BindText(Label label, MainViewModel vm, string propertyName) =>
        label.DataBindings.Add(nameof(Label.Text), vm, propertyName, false, DataSourceUpdateMode.Never);

    private static Color StatusColor(CalculationStatus status) => status switch
    {
        CalculationStatus.Running => Color.Green,
        CalculationStatus.Completed => Color.DarkGreen,
        CalculationStatus.Stopped => Color.Red,
        _ => SystemColors.ControlText,
    };
}
