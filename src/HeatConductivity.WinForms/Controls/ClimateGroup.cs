using HeatConductivity.ViewModels;

namespace HeatConductivity.WinForms.Controls;

/// <summary>Климатические условия: город и три числа, которые город подставляет.</summary>
internal sealed class ClimateGroup : GroupBox
{
    private readonly ComboBox _cities;
    private readonly NumericUpDown _outside;
    private readonly NumericUpDown _inside;
    private readonly NumericUpDown _humidity;

    public ClimateGroup(ToolTip toolTip)
    {
        Text = "Климатические условия (Беларусь)";
        Width = 350;
        Height = 165;
        Font = UiStyle.Bold();

        _cities = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 160, Font = UiStyle.Regular() };

        _outside = UiStyle.CreateNumberInput(
            InputLimits.OutsideTemperatureMin, InputLimits.OutsideTemperatureMax, -24, toolTip,
            "Расчетная температура наиболее холодной пятидневки в Беларуси (СН 2.04.02-2020)");
        _inside = UiStyle.CreateNumberInput(
            InputLimits.InsideTemperatureMin, InputLimits.InsideTemperatureMax, 20, toolTip,
            "Норматив +20°C для жилых помещений в РБ (СанПиН / СН)");
        _humidity = UiStyle.CreateNumberInput(
            InputLimits.HumidityMin, InputLimits.HumidityMax, 55, toolTip,
            "Расчетная относительная влажность воздуха в жилье (50-55% по нормам РБ)");

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 4 };
        layout.Controls.Add(UiStyle.CreateCaption("Город РБ:"), 0, 0);
        layout.Controls.Add(_cities, 1, 0);
        layout.Controls.Add(UiStyle.CreateCaption("Улица (x=0), °C:"), 0, 1);
        layout.Controls.Add(_outside, 1, 1);
        layout.Controls.Add(UiStyle.CreateCaption("Комната (x=L), °C:"), 0, 2);
        layout.Controls.Add(_inside, 1, 2);
        layout.Controls.Add(UiStyle.CreateCaption("Влажность комн., %:"), 0, 3);
        layout.Controls.Add(_humidity, 1, 3);

        Controls.Add(layout);
    }

    public void Bind(MainViewModel vm)
    {
        _cities.Items.AddRange(vm.ClimateItems.Cast<object>().ToArray());
        _cities.DataBindings.Add(
            nameof(ComboBox.SelectedIndex), vm, nameof(MainViewModel.SelectedClimateIndex),
            true, DataSourceUpdateMode.OnPropertyChanged);

        UiStyle.BindValue(_outside, vm, nameof(MainViewModel.OutsideTemperature));
        UiStyle.BindValue(_inside, vm, nameof(MainViewModel.InsideTemperature));
        UiStyle.BindValue(_humidity, vm, nameof(MainViewModel.InsideHumidity));

        DataBindings.Add(nameof(Enabled), vm, nameof(MainViewModel.IsEditingEnabled), false, DataSourceUpdateMode.Never);
    }
}
