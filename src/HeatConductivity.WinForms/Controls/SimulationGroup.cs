using HeatConductivity.ViewModels;

namespace HeatConductivity.WinForms.Controls;

/// <summary>Параметры симуляции: полное время расчёта и шаг интегрирования.</summary>
internal sealed class SimulationGroup : GroupBox
{
    private readonly NumericUpDown _totalTime;
    private readonly NumericUpDown _timeStep;

    public SimulationGroup(ToolTip toolTip)
    {
        Text = "Параметры симуляции";
        Width = 350;
        Height = 100;
        Font = UiStyle.Bold();

        _totalTime = UiStyle.CreateNumberInput(
            InputLimits.TotalTimeMin, InputLimits.TotalTimeMax, 86400, toolTip,
            "Рекомендуется 86400 сек (24 часа)");
        _timeStep = UiStyle.CreateNumberInput(
            InputLimits.TimeStepMin, InputLimits.TimeStepMax, 10, toolTip,
            "Шаг интегрирования (рекомендуется 5-15 сек)");

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
        layout.Controls.Add(UiStyle.CreateCaption("Время расчета (сек):"), 0, 0);
        layout.Controls.Add(_totalTime, 1, 0);
        layout.Controls.Add(UiStyle.CreateCaption("Шаг dt (сек):"), 0, 1);
        layout.Controls.Add(_timeStep, 1, 1);

        Controls.Add(layout);
    }

    public void Bind(MainViewModel vm)
    {
        UiStyle.BindValue(_totalTime, vm, nameof(MainViewModel.TotalTimeSeconds));
        UiStyle.BindValue(_timeStep, vm, nameof(MainViewModel.TimeStepSeconds));

        DataBindings.Add(nameof(Enabled), vm, nameof(MainViewModel.IsEditingEnabled), false, DataSourceUpdateMode.Never);
    }
}
