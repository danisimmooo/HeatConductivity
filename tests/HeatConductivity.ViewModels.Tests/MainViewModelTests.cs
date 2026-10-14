using System.Globalization;
using HeatConductivity.Services.Materials;
using HeatConductivity.Services.Presets;
using HeatConductivity.Services.Reporting;
using Xunit;

namespace HeatConductivity.ViewModels.Tests;

/// <summary>
/// Поведение главной модели: те же числа и тексты, что у исходной формы.
/// Культура инвариантная, время зафиксировано. Контекста синхронизации в тестах нет,
/// поэтому отчёты о прогрессе приходят сразу, в потоке расчёта.
/// </summary>
public class MainViewModelTests
{
    private static readonly DateTime FixedNow = new(2026, 10, 3, 15, 0, 0);

    private static MainViewModel CreateVm(FakeDialogService? dialogs = null, FakeFileService? files = null) =>
        new(
            new InMemoryMaterialRepository(),
            new InMemoryWallPresetRepository(),
            new InMemoryClimatePresetRepository(),
            dialogs ?? new FakeDialogService(),
            files ?? new FakeFileService(),
            CultureInfo.InvariantCulture,
            () => FixedNow);

    // ---------- Начальное состояние ----------

    [Fact]
    public void Initial_state_matches_original_form()
    {
        var vm = CreateVm();

        Assert.Equal(
            new[] { "Блоки газосиликатные (D500)", "Минеральная вата (Фасад)", "Штукатурка защитно-отделочная" },
            vm.Layers.Select(l => l.Material.Name));
        Assert.Equal(new double[] { 400, 100, 15 }, vm.Layers.Select(l => l.ThicknessMm));

        Assert.Equal(-24, vm.OutsideTemperature);
        Assert.Equal(20, vm.InsideTemperature);
        Assert.Equal(55, vm.InsideHumidity);
        Assert.Equal(86400, vm.TotalTimeSeconds);
        Assert.Equal(10, vm.TimeStepSeconds);

        Assert.Equal("R = 5.764 м²·К/Вт", vm.ResistanceText);
        Assert.Equal("q = 7.6 Вт/м²", vm.HeatFluxText);
        Assert.Equal("Точка росы: 11.0 °C", vm.DewPointText);
        Assert.True(vm.IsDewPointAboveZero);
        Assert.Equal("Датчик: X = 0.26 м | T = 20.0 °C", vm.SensorInfoText);

        Assert.Equal("Статус: Готов", vm.StatusText);
        Assert.Equal(CalculationStatus.Ready, vm.CalcStatus);
        Assert.Equal("Шагов: 0", vm.StepsText);
        Assert.Equal("Время: 00:00:00", vm.ElapsedText);
        Assert.True(vm.CanStart);
        Assert.False(vm.CanStop);
        Assert.True(vm.IsEditingEnabled);
    }

    [Fact]
    public void Initial_charts_data_is_prepared()
    {
        var vm = CreateVm();

        Assert.Equal(-24, vm.HeatMap.MinTemperature);
        Assert.Equal(20, vm.HeatMap.MaxTemperature);
        Assert.Equal(50, vm.HeatMap.SensorX);
        Assert.Equal(25, vm.HeatMap.SensorY);

        Assert.Equal(101, vm.ProfilePoints.Count);
        Assert.Equal(-24, vm.ProfilePoints[0].Temperature);
        Assert.Equal(20, vm.ProfilePoints[100].Temperature);

        Assert.Equal(3, vm.LayerResistances.Count);
        Assert.Equal(2.5, vm.LayerResistances[1], 9);
        Assert.Empty(vm.SensorHistory);
    }

    [Fact]
    public void Lists_for_combo_boxes_match_original_items()
    {
        var vm = CreateVm();

        Assert.Equal(5, vm.WallPresetItems.Count);
        Assert.Equal("-- Типовая стена (Беларусь) --", vm.WallPresetItems[0]);
        Assert.Equal("Брус с утеплением минватой", vm.WallPresetItems[4]);
        Assert.Equal(6, vm.ClimateItems.Count);
        Assert.Equal("Минск (-24°C)", vm.ClimateItems[0]);
        Assert.Equal(7, vm.AvailableMaterials.Count);
    }

    // ---------- Слои ----------

    [Fact]
    public void Changing_material_substitutes_recommended_thickness_and_recalculates()
    {
        var vm = CreateVm();
        var concrete = vm.AvailableMaterials.First(m => m.Name == "Железобетон");

        vm.Layers[2].Material = concrete;

        Assert.Equal(200, vm.Layers[2].ThicknessMm);
        Assert.Equal("R = 5.865 м²·К/Вт", vm.ResistanceText);
    }

    [Fact]
    public void Changing_thickness_recalculates()
    {
        var vm = CreateVm();

        vm.Layers[1].ThicknessMm = 200;   // минвата 200 мм: R = 0.17 + 3.0769 + 5.0 + 0.0172

        Assert.Equal("R = 8.264 м²·К/Вт", vm.ResistanceText);
    }

    [Fact]
    public void AddLayer_appends_mineral_wool_100_mm()
    {
        var vm = CreateVm();

        vm.AddLayerCommand.Execute(null);

        Assert.Equal(4, vm.Layers.Count);
        Assert.Equal("Минеральная вата (Фасад)", vm.Layers[3].Material.Name);
        Assert.Equal(100, vm.Layers[3].ThicknessMm);
        Assert.Equal("R = 8.264 м²·К/Вт", vm.ResistanceText);
    }

    [Fact]
    public void RemoveLayer_without_selection_removes_the_last_layer()
    {
        var vm = CreateVm();

        vm.RemoveLayerCommand.Execute(null);

        Assert.Equal(2, vm.Layers.Count);
        Assert.Equal("R = 5.747 м²·К/Вт", vm.ResistanceText);
    }

    [Fact]
    public void RemoveLayer_removes_the_selected_layer()
    {
        var vm = CreateVm();
        vm.SelectedLayer = vm.Layers[0];

        vm.RemoveLayerCommand.Execute(null);

        Assert.Equal(
            new[] { "Минеральная вата (Фасад)", "Штукатурка защитно-отделочная" },
            vm.Layers.Select(l => l.Material.Name));
        Assert.Null(vm.SelectedLayer);
        Assert.Equal("R = 2.687 м²·К/Вт", vm.ResistanceText);
    }

    [Fact]
    public void Removing_all_layers_keeps_the_model_working_and_disables_remove()
    {
        var vm = CreateVm();

        for (int i = 0; i < 3; i++) vm.RemoveLayerCommand.Execute(null);

        Assert.Empty(vm.Layers);
        Assert.Equal("R = 0.170 м²·К/Вт", vm.ResistanceText);
        Assert.False(vm.RemoveLayerCommand.CanExecute(null));
        Assert.Equal(101, vm.ProfilePoints.Count);
    }

    [Fact]
    public void Layers_with_non_positive_thickness_are_ignored_in_calculation()
    {
        var vm = CreateVm();

        vm.Layers[1].ThicknessMm = 0;   // минвата «исчезла»

        Assert.Equal("R = 3.264 м²·К/Вт", vm.ResistanceText);
        Assert.Equal(2, vm.LayerResistances.Count);
    }

    // ---------- Шаблоны и климат ----------

    [Fact]
    public void Selecting_a_wall_preset_replaces_all_layers()
    {
        var vm = CreateVm();

        vm.SelectedWallPresetIndex = 3;   // панельный дом

        Assert.Equal(
            new[] { "Железобетон", "Минеральная вата (Фасад)", "Железобетон" },
            vm.Layers.Select(l => l.Material.Name));
        Assert.Equal(new double[] { 160, 140, 80 }, vm.Layers.Select(l => l.ThicknessMm));
        Assert.Equal("R = 3.811 м²·К/Вт", vm.ResistanceText);
    }

    [Fact]
    public void Selecting_the_placeholder_keeps_current_layers()
    {
        var vm = CreateVm();
        vm.SelectedWallPresetIndex = 3;

        vm.SelectedWallPresetIndex = 0;

        Assert.Equal(3, vm.Layers.Count);
        Assert.Equal("R = 3.811 м²·К/Вт", vm.ResistanceText);
    }

    [Fact]
    public void Selecting_a_city_applies_its_climate()
    {
        var vm = CreateVm();

        vm.SelectedClimateIndex = 2;   // Витебск

        Assert.Equal(-25, vm.OutsideTemperature);
        Assert.Equal(20, vm.InsideTemperature);
        Assert.Equal(50, vm.InsideHumidity);
        Assert.Equal("q = 7.8 Вт/м²", vm.HeatFluxText);
        Assert.Equal("Точка росы: 10.0 °C", vm.DewPointText);
    }

    [Fact]
    public void Changing_outside_temperature_updates_heat_flux_and_notifies()
    {
        var vm = CreateVm();
        var changed = new List<string?>();
        vm.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        vm.OutsideTemperature = -30;

        Assert.Equal("q = 8.7 Вт/м²", vm.HeatFluxText);
        Assert.Contains(nameof(MainViewModel.HeatFluxText), changed);
        Assert.Contains(nameof(MainViewModel.HeatMap), changed);
    }

    [Fact]
    public void Heat_map_range_is_widened_when_outside_is_not_colder_than_inside()
    {
        var vm = CreateVm();

        vm.OutsideTemperature = 25;   // внутри 20

        Assert.Equal(25, vm.HeatMap.MinTemperature);
        Assert.Equal(26, vm.HeatMap.MaxTemperature);
    }

    // ---------- Датчик ----------

    [Fact]
    public void SelectSensorAt_maps_relative_click_to_grid_node()
    {
        var vm = CreateVm();

        vm.SelectSensorAt(0.015, 0.5);

        Assert.Equal(1, vm.HeatMap.SensorX);
        Assert.Equal(25, vm.HeatMap.SensorY);
        Assert.Equal("Датчик: X = 0.01 м | T = 20.0 °C", vm.SensorInfoText);
    }

    [Theory]
    [InlineData(2.0, -1.0, 100, 50)]
    [InlineData(-1.0, 2.0, 0, 0)]
    public void SelectSensorAt_clamps_clicks_outside_the_map(double relX, double relY, int expectedX, int expectedY)
    {
        var vm = CreateVm();

        vm.SelectSensorAt(relX, relY);

        Assert.Equal(expectedX, vm.HeatMap.SensorX);
        Assert.Equal(expectedY, vm.HeatMap.SensorY);
    }

    // ---------- Расчёт динамики ----------

    private static async Task<MainViewModel> RunShortSimulationAsync(Action<MainViewModel>? beforeStart = null)
    {
        var vm = CreateVm();
        vm.TotalTimeSeconds = 1000;
        vm.TimeStepSeconds = 10;
        vm.SelectSensorAt(0.015, 0.5);   // узел (1, 25) у холодной границы
        beforeStart?.Invoke(vm);
        await vm.StartAsync();
        return vm;
    }

    [Fact]
    public async Task Simulation_fills_sensor_history_with_reference_values()
    {
        var vm = await RunShortSimulationAsync();

        Assert.Equal(5, vm.SensorHistory.Count);

        Assert.Equal(200.0 / 3600.0, vm.SensorHistory[0].Hours, 12);
        Assert.Equal(-8.461670177102976, vm.SensorHistory[0].Temperature, 9);

        Assert.Equal(1000.0 / 3600.0, vm.SensorHistory[^1].Hours, 12);
        Assert.Equal(-16.79890339004931, vm.SensorHistory[^1].Temperature, 9);
    }

    [Fact]
    public async Task Simulation_updates_texts_and_final_state()
    {
        var vm = await RunShortSimulationAsync();

        Assert.Equal(CalculationStatus.Completed, vm.CalcStatus);
        Assert.Equal("Статус: Завершено", vm.StatusText);
        Assert.Equal("Шаг: 100 / 100", vm.StepsText);
        Assert.StartsWith("Время: 00:", vm.ElapsedText);
        Assert.False(vm.IsRunning);
        Assert.True(vm.IsEditingEnabled);
        Assert.True(vm.CanStart);

        Assert.Equal(-16.79890339004931, vm.HeatMap.Temperature[1, 25], 9);
        Assert.Equal(-16.79890339004931, vm.ProfilePoints[1].Temperature, 9);
        Assert.Equal("Датчик: X = 0.01 м | T = -16.8 °C", vm.SensorInfoText);
    }

    [Fact]
    public async Task Inputs_are_locked_and_commands_are_disabled_while_running()
    {
        var vm = CreateVm();
        vm.TotalTimeSeconds = 1000;
        vm.TimeStepSeconds = 10;

        var snapshots = new List<(bool Editing, bool StartEnabled, bool StopEnabled, bool AddEnabled)>();
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.StepsText))
            {
                snapshots.Add((
                    vm.IsEditingEnabled,
                    vm.StartCommand.CanExecute(null),
                    vm.StopCommand.CanExecute(null),
                    vm.AddLayerCommand.CanExecute(null)));
            }
        };

        await vm.StartAsync();

        Assert.Equal(5, snapshots.Count);
        Assert.All(snapshots, s =>
        {
            Assert.False(s.Editing);
            Assert.False(s.StartEnabled);
            Assert.True(s.StopEnabled);
            Assert.False(s.AddEnabled);
        });
        Assert.False(vm.StopCommand.CanExecute(null));
        Assert.True(vm.AddLayerCommand.CanExecute(null));
    }

    [Fact]
    public async Task Second_run_starts_from_the_initial_state_again()
    {
        var vm = await RunShortSimulationAsync();

        await vm.StartAsync();

        Assert.Equal(5, vm.SensorHistory.Count);
        Assert.Equal(-8.461670177102976, vm.SensorHistory[0].Temperature, 9);
        Assert.Equal(-16.79890339004931, vm.SensorHistory[^1].Temperature, 9);
    }

    [Fact]
    public async Task Selecting_a_new_sensor_clears_the_history()
    {
        var vm = await RunShortSimulationAsync();

        vm.SelectSensorAt(0.5, 0.5);

        Assert.Empty(vm.SensorHistory);
    }

    [Fact]
    public async Task Stop_ends_the_run_early_and_keeps_stopped_status()
    {
        var vm = CreateVm();
        vm.TotalTimeSeconds = 100_000;
        vm.TimeStepSeconds = 10;

        bool stopRequested = false;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.StepsText) && !stopRequested)
            {
                stopRequested = true;
                vm.Stop();
            }
        };

        await vm.StartAsync();

        Assert.Equal(CalculationStatus.Stopped, vm.CalcStatus);
        Assert.Equal("Статус: Остановлено", vm.StatusText);
        Assert.Equal("Шаг: 20 / 10000", vm.StepsText);
        Assert.Single(vm.SensorHistory);
        Assert.False(vm.IsRunning);
    }

    [Fact]
    public void Stop_when_not_running_does_nothing()
    {
        var vm = CreateVm();

        vm.Stop();

        Assert.Equal(CalculationStatus.Ready, vm.CalcStatus);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1000, 0)]
    public async Task Start_is_disabled_for_non_positive_time_or_step(double totalTime, double dt)
    {
        var vm = CreateVm();
        vm.TotalTimeSeconds = totalTime;
        vm.TimeStepSeconds = dt;

        Assert.False(vm.CanStart);
        Assert.False(vm.StartCommand.CanExecute(null));

        await vm.StartAsync();

        Assert.Equal(CalculationStatus.Ready, vm.CalcStatus);
        Assert.Empty(vm.SensorHistory);
    }

    // ---------- Экспорт отчёта ----------

    [Fact]
    public void Export_writes_the_report_and_informs_the_user()
    {
        var dialogs = new FakeDialogService { PathToReturn = "report.txt" };
        var files = new FakeFileService();
        var vm = CreateVm(dialogs, files);
        vm.SelectedClimateIndex = 2;   // Витебск

        vm.ExportReportCommand.Execute(null);

        Assert.Equal("Расчет_Теплотехника_РБ_20261003_1500.txt", dialogs.LastSuggestedName);
        Assert.Equal(ReportBuilder.FileFilter, dialogs.LastFilter);

        var report = files.Written["report.txt"].Replace("\r\n", "\n");
        Assert.Contains("Дата проведения расчета: 03.10.2026 15:00:00\n", report);
        Assert.Contains("   Населенный пункт: Витебск (-25°C)\n", report);
        Assert.Contains("   Наружная температура (x=0): -25 °C\n", report);
        Assert.Contains("   Слой 2: Минеральная вата (Фасад) - 100 мм\n", report);
        Assert.Contains("   Положение датчика контроля: Датчик: X = 0.26 м | T = 20.0 °C\n", report);

        var message = Assert.Single(dialogs.Messages);
        Assert.Equal("Экспорт завершён", message.Title);
        Assert.Contains("report.txt", message.Message);
    }

    [Fact]
    public void Export_does_nothing_when_the_user_cancels_the_dialog()
    {
        var dialogs = new FakeDialogService { PathToReturn = null };
        var files = new FakeFileService();
        var vm = CreateVm(dialogs, files);

        vm.ExportReportCommand.Execute(null);

        Assert.Empty(files.Written);
        Assert.Empty(dialogs.Messages);
    }

    [Fact]
    public void Export_reports_io_errors_instead_of_crashing()
    {
        var dialogs = new FakeDialogService { PathToReturn = "report.txt" };
        var files = new FakeFileService { ThrowIoError = true };
        var vm = CreateVm(dialogs, files);

        vm.ExportReportCommand.Execute(null);

        var message = Assert.Single(dialogs.Messages);
        Assert.Equal("Ошибка экспорта", message.Title);
        Assert.Contains("Диск недоступен", message.Message);
    }
}
