using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using HeatConductivity.Core.Calculations;
using HeatConductivity.Core.Models;
using HeatConductivity.Core.Simulation;
using HeatConductivity.Services.Abstractions;
using HeatConductivity.Services.Materials;
using HeatConductivity.Services.Presets;
using HeatConductivity.Services.Reporting;
using HeatConductivity.ViewModels.Infrastructure;

namespace HeatConductivity.ViewModels;

/// <summary>
/// Модель главного окна: слои стены, климат, параметры симуляции, результаты расчёта,
/// запуск и остановка динамики, датчик, экспорт отчёта. Не зависит от WinForms.
/// </summary>
public sealed class MainViewModel : ObservableObject
{
    /// <summary>Первый пункт списка шаблонов: «ничего не выбрано».</summary>
    public const string WallPresetPlaceholder = "-- Типовая стена (Беларусь) --";

    private const int DefaultSensorX = 50;
    private const int DefaultSensorY = 25;
    private const string InitialStepsText = "Шагов: 0";
    private const string InitialElapsedText = "Время: 00:00:00";

    private readonly IMaterialRepository _materials;
    private readonly IWallPresetRepository _wallPresets;
    private readonly IClimatePresetRepository _climatePresets;
    private readonly IDialogService _dialogs;
    private readonly IFileService _files;
    private readonly IFormatProvider? _culture;
    private readonly Func<DateTime> _now;
    private readonly Stopwatch _stopwatch = new();

    private ThermalField? _field;
    private double[,] _currentTemperature = new double[0, 0];
    private ClimateConditions _modelClimate = new(0, 0, 0);
    private CancellationTokenSource? _cts;
    private int _suspendRecalculation;
    private int _sensorX = DefaultSensorX;
    private int _sensorY = DefaultSensorY;

    private double _outsideTemperature;
    private double _insideTemperature;
    private double _insideHumidity;
    private double _totalTimeSeconds = 86400;
    private double _timeStepSeconds = 10;
    private int _selectedWallPresetIndex;
    private int _selectedClimateIndex;
    private LayerViewModel? _selectedLayer;
    private bool _isRunning;
    private CalculationStatus _calcStatus = CalculationStatus.Ready;

    private string _resistanceText = "";
    private string _heatFluxText = "";
    private string _dewPointText = "";
    private bool _isDewPointAboveZero;
    private string _sensorInfoText = "";
    private string _stepsText = InitialStepsText;
    private string _elapsedText = InitialElapsedText;
    private string _adviceText = "";
    private string _moistureAnalysisText = "";
    private HeatMapFrame _heatMap = null!;
    private IReadOnlyList<ProfilePoint> _profilePoints = Array.Empty<ProfilePoint>();
    private IReadOnlyList<double> _layerResistances = Array.Empty<double>();

    /// <param name="culture">Культура форматирования чисел в текстах; по умолчанию — текущая.</param>
    /// <param name="now">Источник текущего времени (для имени файла и даты отчёта); по умолчанию — часы системы.</param>
    public MainViewModel(
        IMaterialRepository materials,
        IWallPresetRepository wallPresets,
        IClimatePresetRepository climatePresets,
        IDialogService dialogs,
        IFileService files,
        IFormatProvider? culture = null,
        Func<DateTime>? now = null)
    {
        _materials = materials;
        _wallPresets = wallPresets;
        _climatePresets = climatePresets;
        _dialogs = dialogs;
        _files = files;
        _culture = culture;
        _now = now ?? (() => DateTime.Now);

        AvailableMaterials = materials.GetAll();
        WallPresetItems = new[] { WallPresetPlaceholder }
            .Concat(wallPresets.GetAll().Select(p => p.Name))
            .ToList();
        ClimateItems = climatePresets.GetAll().Select(p => p.DisplayName).ToList();

        var climate = climatePresets.DefaultPreset;
        _outsideTemperature = climate.OutsideTemperature;
        _insideTemperature = climate.InsideTemperature;
        _insideHumidity = climate.InsideHumidity;

        AddLayerCommand = new RelayCommand(AddLayer, () => !IsRunning);
        RemoveLayerCommand = new RelayCommand(RemoveLayer, () => !IsRunning && Layers.Count > 0);
        StartCommand = new AsyncRelayCommand(StartAsync, () => CanStart);
        StopCommand = new RelayCommand(Stop, () => IsRunning);
        ExportReportCommand = new RelayCommand(ExportReport);

        Layers = new BindingList<LayerViewModel>();
        Layers.ListChanged += OnLayersChanged;

        _suspendRecalculation++;
        ReplaceLayers(CreateLayers(wallPresets.DefaultPreset));
        _suspendRecalculation--;

        Recalculate();
    }

    // ---------- Данные для списков ----------

    public IReadOnlyList<Material> AvailableMaterials { get; }

    /// <summary>Пункты списка шаблонов: заглушка и четыре типовые стены.</summary>
    public IReadOnlyList<string> WallPresetItems { get; }

    /// <summary>Подписи городов, например «Минск (-24°C)».</summary>
    public IReadOnlyList<string> ClimateItems { get; }

    // ---------- Ввод ----------

    /// <summary>Слои стены от улицы к комнате.</summary>
    public BindingList<LayerViewModel> Layers { get; }

    /// <summary>Выбранный в таблице слой (его удаляет команда «Удалить»).</summary>
    public LayerViewModel? SelectedLayer
    {
        get => _selectedLayer;
        set => SetProperty(ref _selectedLayer, value);
    }

    public int SelectedWallPresetIndex
    {
        get => _selectedWallPresetIndex;
        set
        {
            if (value < 0 || value >= WallPresetItems.Count) return;
            if (SetProperty(ref _selectedWallPresetIndex, value) && value > 0)
                ReplaceLayers(CreateLayers(_wallPresets.GetAll()[value - 1]));
        }
    }

    public int SelectedClimateIndex
    {
        get => _selectedClimateIndex;
        set
        {
            if (value < 0 || value >= ClimateItems.Count) return;
            if (SetProperty(ref _selectedClimateIndex, value))
                ApplyClimate(_climatePresets.GetAll()[value]);
        }
    }

    /// <summary>Температура на улице (x = 0), °C.</summary>
    public double OutsideTemperature
    {
        get => _outsideTemperature;
        set { if (SetProperty(ref _outsideTemperature, value)) Recalculate(); }
    }

    /// <summary>Температура в комнате (x = L), °C.</summary>
    public double InsideTemperature
    {
        get => _insideTemperature;
        set { if (SetProperty(ref _insideTemperature, value)) Recalculate(); }
    }

    /// <summary>Относительная влажность в комнате, %.</summary>
    public double InsideHumidity
    {
        get => _insideHumidity;
        set { if (SetProperty(ref _insideHumidity, value)) Recalculate(); }
    }

    /// <summary>Время расчёта динамики, с.</summary>
    public double TotalTimeSeconds
    {
        get => _totalTimeSeconds;
        set
        {
            if (!SetProperty(ref _totalTimeSeconds, value)) return;
            OnStartConditionsChanged();
        }
    }

    /// <summary>Шаг интегрирования dt, с.</summary>
    public double TimeStepSeconds
    {
        get => _timeStepSeconds;
        set
        {
            if (!SetProperty(ref _timeStepSeconds, value)) return;
            OnStartConditionsChanged();
        }
    }

    // ---------- Результаты ----------

    public string ResistanceText { get => _resistanceText; private set => SetProperty(ref _resistanceText, value); }
    public string HeatFluxText { get => _heatFluxText; private set => SetProperty(ref _heatFluxText, value); }
    public string DewPointText { get => _dewPointText; private set => SetProperty(ref _dewPointText, value); }

    /// <summary>Точка росы выше нуля — View подкрашивает подпись предупреждающим цветом.</summary>
    public bool IsDewPointAboveZero { get => _isDewPointAboveZero; private set => SetProperty(ref _isDewPointAboveZero, value); }

    public string SensorInfoText { get => _sensorInfoText; private set => SetProperty(ref _sensorInfoText, value); }
    public string AdviceText { get => _adviceText; private set => SetProperty(ref _adviceText, value); }
    public string MoistureAnalysisText { get => _moistureAnalysisText; private set => SetProperty(ref _moistureAnalysisText, value); }

    /// <summary>Текущий кадр тепловой карты.</summary>
    public HeatMapFrame HeatMap { get => _heatMap; private set => SetProperty(ref _heatMap, value); }

    /// <summary>Профиль T(x) вдоль средней линии стены.</summary>
    public IReadOnlyList<ProfilePoint> ProfilePoints { get => _profilePoints; private set => SetProperty(ref _profilePoints, value); }

    /// <summary>Сопротивления слоёв R (по порядку слоёв) для столбчатой диаграммы.</summary>
    public IReadOnlyList<double> LayerResistances { get => _layerResistances; private set => SetProperty(ref _layerResistances, value); }

    /// <summary>История температуры в точке датчика за текущий расчёт.</summary>
    public ObservableCollection<SensorPoint> SensorHistory { get; } = new();

    // ---------- Состояние расчёта ----------

    public bool IsRunning
    {
        get => _isRunning;
        private set
        {
            if (!SetProperty(ref _isRunning, value)) return;

            OnPropertyChanged(nameof(IsEditingEnabled));
            OnPropertyChanged(nameof(CanStart));
            OnPropertyChanged(nameof(CanStop));
            RefreshCommands();
        }
    }

    /// <summary>Во время расчёта входные данные заблокированы: поле считается по зафиксированной модели.</summary>
    public bool IsEditingEnabled => !IsRunning;

    public bool CanStart => !IsRunning && TotalTimeSeconds > 0 && TimeStepSeconds > 0;
    public bool CanStop => IsRunning;

    public CalculationStatus CalcStatus
    {
        get => _calcStatus;
        private set
        {
            if (SetProperty(ref _calcStatus, value))
                OnPropertyChanged(nameof(StatusText));
        }
    }

    public string StatusText => CalcStatus switch
    {
        CalculationStatus.Running => "Статус: Расчет динамики...",
        CalculationStatus.Completed => "Статус: Завершено",
        CalculationStatus.Stopped => "Статус: Остановлено",
        _ => "Статус: Готов",
    };

    public string StepsText { get => _stepsText; private set => SetProperty(ref _stepsText, value); }
    public string ElapsedText { get => _elapsedText; private set => SetProperty(ref _elapsedText, value); }

    // ---------- Команды ----------

    public RelayCommand AddLayerCommand { get; }
    public RelayCommand RemoveLayerCommand { get; }
    public AsyncRelayCommand StartCommand { get; }
    public RelayCommand StopCommand { get; }
    public RelayCommand ExportReportCommand { get; }

    // ---------- Действия ----------

    /// <summary>
    /// Выбор точки датчика по относительным координатам клика на тепловой карте:
    /// (0, 0) — левый верхний угол, (1, 1) — правый нижний. Значения вне карты прижимаются к краю.
    /// </summary>
    public void SelectSensorAt(double relativeX, double relativeY)
    {
        if (_field == null) return;

        _sensorX = Math.Clamp((int)(relativeX * _field.Nx), 0, _field.Nx - 1);
        _sensorY = Math.Clamp((int)((1.0 - relativeY) * _field.Ny), 0, _field.Ny - 1);

        UpdateSensorInfo();
        SensorHistory.Clear();
        HeatMap = CreateFrame(_currentTemperature);
    }

    /// <summary>Пересобирает модель и считает динамику. Можно прервать через <see cref="Stop"/>.</summary>
    public async Task StartAsync()
    {
        if (!CanStart) return;

        Recalculate();

        var field = _field!;
        var solver = new HeatSolver2D(field);
        double totalTime = TotalTimeSeconds;
        double dt = TimeStepSeconds;

        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        IsRunning = true;
        CalcStatus = CalculationStatus.Running;
        StepsText = InitialStepsText;
        ElapsedText = InitialElapsedText;
        SensorHistory.Clear();
        _stopwatch.Restart();

        // Контекст берём здесь: метод вызывается из UI-потока, поэтому отчёты вернутся в него.
        var progress = new ContextProgress<SimulationProgress>(SynchronizationContext.Current, OnProgress);

        try
        {
            await Task.Run(() => solver.Run(totalTime, dt, progress, token));
        }
        finally
        {
            _stopwatch.Stop();
            CalcStatus = token.IsCancellationRequested ? CalculationStatus.Stopped : CalculationStatus.Completed;
            IsRunning = false;
            _cts?.Dispose();
            _cts = null;
        }
    }

    public void Stop()
    {
        if (!IsRunning) return;

        _cts?.Cancel();
        CalcStatus = CalculationStatus.Stopped;
    }

    // ---------- Слои ----------

    private void AddLayer()
    {
        var template = _wallPresets.NewLayerDefault;
        var material = _materials.FindByName(template.MaterialName);
        if (material == null) return;

        Layers.Add(new LayerViewModel(material, template.ThicknessMm));
    }

    private void RemoveLayer()
    {
        if (Layers.Count == 0) return;

        var target = SelectedLayer != null && Layers.Contains(SelectedLayer)
            ? SelectedLayer
            : Layers[Layers.Count - 1];

        Layers.Remove(target);
        SelectedLayer = null;
    }

    private IEnumerable<LayerViewModel> CreateLayers(WallPreset preset) =>
        preset.CreateLayers(_materials).Select(l => new LayerViewModel(l.Material, l.ThicknessMm));

    /// <summary>Заменяет все слои и пересчитывает один раз, а не по разу на каждый слой.</summary>
    private void ReplaceLayers(IEnumerable<LayerViewModel> items)
    {
        Layers.RaiseListChangedEvents = false;
        try
        {
            Layers.Clear();
            foreach (var item in items)
                Layers.Add(item);
        }
        finally
        {
            Layers.RaiseListChangedEvents = true;
        }

        SelectedLayer = null;
        Layers.ResetBindings();
    }

    private void OnLayersChanged(object? sender, ListChangedEventArgs e)
    {
        RemoveLayerCommand.RaiseCanExecuteChanged();
        Recalculate();
    }

    // ---------- Климат ----------

    private void ApplyClimate(ClimatePreset preset)
    {
        _suspendRecalculation++;
        try
        {
            OutsideTemperature = preset.OutsideTemperature;
            InsideTemperature = preset.InsideTemperature;
            InsideHumidity = preset.InsideHumidity;
        }
        finally
        {
            _suspendRecalculation--;
        }

        Recalculate();
    }

    // ---------- Расчёт ----------

    private WallConstruction BuildWall() =>
        new(Layers
            .Where(l => l.ThicknessMm > 0)
            .Select(l => new Layer(l.Material, l.ThicknessMm)));

    private ClimateConditions BuildClimate() =>
        new(OutsideTemperature, InsideTemperature, InsideHumidity);

    /// <summary>Пересчитывает показатели, тексты и пересобирает расчётную сетку (аналог InitModelFromGrid).</summary>
    private void Recalculate()
    {
        if (_suspendRecalculation > 0 || IsRunning) return;

        var wall = BuildWall();
        var climate = BuildClimate();

        double rTotal = ThermalResistanceCalculator.TotalResistance(wall);
        double heatFlux = ThermalResistanceCalculator.HeatFlux(wall, climate);
        double dewPoint = DewPointCalculator.Calculate(climate.InsideTemperature, climate.InsideHumidity);

        ResistanceText = ResultFormatter.Resistance(rTotal, _culture);
        HeatFluxText = ResultFormatter.HeatFlux(heatFlux, _culture);
        DewPointText = ResultFormatter.DewPoint(dewPoint, _culture);
        IsDewPointAboveZero = dewPoint > 0;
        AdviceText = EngineeringAdviceFormatter.Format(wall, climate, _culture);
        MoistureAnalysisText = MoistureAnalysisFormatter.Format(wall, climate, _culture);
        LayerResistances = LayerTemperatureProfile.Calculate(wall, climate).Select(l => l.Resistance).ToList();

        _modelClimate = climate;
        _field = ThermalFieldBuilder.Build(wall, climate, GridSettings.Default, _materials.GetAll().FirstOrDefault());
        _currentTemperature = (double[,])_field.Temperature.Clone();

        ProfilePoints = CreateProfile(_currentTemperature);
        HeatMap = CreateFrame(_currentTemperature);
        UpdateSensorInfo();
    }

    private void OnProgress(SimulationProgress progress)
    {
        _currentTemperature = progress.Temperature;

        StepsText = $"Шаг: {progress.Step} / {progress.TotalSteps}";
        ElapsedText = "Время: " + _stopwatch.Elapsed.ToString(@"hh\:mm\:ss");

        UpdateSensorInfo();
        SensorHistory.Add(new SensorPoint(progress.TimeSeconds / 3600.0, progress.Temperature[_sensorX, _sensorY]));
        HeatMap = CreateFrame(progress.Temperature);
        ProfilePoints = CreateProfile(progress.Temperature);
    }

    private void UpdateSensorInfo()
    {
        if (_field == null) return;

        SensorInfoText = ResultFormatter.Sensor(
            _field.XAt(_sensorX),
            _currentTemperature[_sensorX, _sensorY],
            _culture);
    }

    private HeatMapFrame CreateFrame(double[,] temperature)
    {
        double min = _modelClimate.OutsideTemperature;
        double max = _modelClimate.InsideTemperature;
        if (min >= max) max = min + 1.0;

        return new HeatMapFrame(temperature, min, max, _sensorX, _sensorY);
    }

    private ProfilePoint[] CreateProfile(double[,] temperature)
    {
        var field = _field!;
        int midY = field.Ny / 2;
        double dx = field.Dx;

        var points = new ProfilePoint[field.Nx];
        for (int i = 0; i < points.Length; i++)
            points[i] = new ProfilePoint(i * dx, temperature[i, midY]);

        return points;
    }

    // ---------- Отчёт ----------

    private void ExportReport()
    {
        var now = _now();
        string? path = _dialogs.PromptSaveFile(ReportBuilder.SuggestFileName(now), ReportBuilder.FileFilter);
        if (path == null) return;

        var data = new ReportData(
            now,
            BuildWall(),
            ClimateItems[SelectedClimateIndex],
            BuildClimate(),
            _field!.XAt(_sensorX),
            _currentTemperature[_sensorX, _sensorY]);

        try
        {
            _files.WriteAllText(path, ReportBuilder.Build(data, _culture));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _dialogs.ShowInfo("Ошибка экспорта", $"Не удалось сохранить отчёт:\n{ex.Message}");
            return;
        }

        _dialogs.ShowInfo("Экспорт завершён", $"Отчёт сохранён по нормам РБ:\n{path}");
    }

    // ---------- Служебное ----------

    private void OnStartConditionsChanged()
    {
        OnPropertyChanged(nameof(CanStart));
        StartCommand.RaiseCanExecuteChanged();
    }

    private void RefreshCommands()
    {
        AddLayerCommand.RaiseCanExecuteChanged();
        RemoveLayerCommand.RaiseCanExecuteChanged();
        StartCommand.RaiseCanExecuteChanged();
        StopCommand.RaiseCanExecuteChanged();
    }
}
