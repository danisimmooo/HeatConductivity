using HeatConductivity.ViewModels;
using HeatConductivity.WinForms.Controls;

namespace HeatConductivity.WinForms;

/// <summary>
/// Главное окно: только раскладка и привязка к <see cref="MainViewModel"/>.
/// Расчётов и бизнес-логики здесь нет.
/// </summary>
internal sealed class MainForm : Form
{
    private readonly MainViewModel _vm;

    private readonly ToolTip _toolTip = new()
    {
        ToolTipTitle = "Нормы Республики Беларусь (СН / СНиП)",
        IsBalloon = true,
        AutoPopDelay = 10000,
        InitialDelay = 300,
        ReshowDelay = 100,
    };

    public MainForm(MainViewModel vm)
    {
        _vm = vm;

        Text = "Инженерный расчёт теплопроводности и теплопотерь (Нормы Республики Беларусь)";
        Size = new Size(1400, 880);
        MinimumSize = new Size(1280, 780);
        StartPosition = FormStartPosition.CenterScreen;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Padding = new Padding(8),
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 370f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 310f));

        layout.Controls.Add(BuildInputPanel(), 0, 0);
        layout.Controls.Add(BuildCenterTabs(), 1, 0);
        layout.Controls.Add(BuildResultsPanel(), 2, 0);

        Controls.Add(layout);
    }

    // ---------- Левая панель: ввод данных ----------

    private Control BuildInputPanel()
    {
        var layers = new LayersGroup(_toolTip);
        var climate = new ClimateGroup(_toolTip);
        var simulation = new SimulationGroup(_toolTip);
        var runButtons = new RunButtonsPanel();

        layers.Bind(_vm);
        climate.Bind(_vm);
        simulation.Bind(_vm);
        runButtons.Bind(_vm);

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
        };
        panel.Controls.Add(layers);
        panel.Controls.Add(climate);
        panel.Controls.Add(simulation);
        panel.Controls.Add(runButtons);
        return panel;
    }

    // ---------- Центр: вкладки с картой и графиками ----------

    private Control BuildCenterTabs()
    {
        var tabs = new TabControl { Dock = DockStyle.Fill, Font = UiStyle.Regular(9f) };

        var fieldTab = new TabPage("🔥 2D Тепловое поле и Датчик");
        fieldTab.Controls.Add(BuildFieldTab());
        tabs.TabPages.Add(fieldTab);

        var analyticsTab = new TabPage("📊 Аналитика и Градиент T(x)");
        analyticsTab.Controls.Add(BuildAnalyticsTab());
        tabs.TabPages.Add(analyticsTab);

        return tabs;
    }

    private Control BuildFieldTab()
    {
        var heatMap = new HeatMapControl();
        heatMap.RelativeClick += _vm.SelectSensorAt;
        Observe(nameof(MainViewModel.HeatMap), () => heatMap.Frame = _vm.HeatMap);

        var sensorChart = new SensorChartControl();
        sensorChart.Bind(_vm.SensorHistory);

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 55f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 45f));
        layout.Controls.Add(
            CreateGroup("Тепловое поле (Кликните мышью для выбора точки датчика)", heatMap), 0, 0);
        layout.Controls.Add(
            CreateGroup("Динамика температуры в точке датчика T(t)", sensorChart), 0, 1);
        return layout;
    }

    private Control BuildAnalyticsTab()
    {
        var profileChart = new ProfileChartControl();
        Observe(nameof(MainViewModel.ProfilePoints), () => profileChart.Points = _vm.ProfilePoints);

        var resistanceChart = new LayerResistanceChartControl();
        Observe(nameof(MainViewModel.LayerResistances), () => resistanceChart.Resistances = _vm.LayerResistances);

        var moisture = new TextOutputGroup(
            "Анализ влажностного режима и точки росы (СН 2.04.01-2020)", UiStyle.Monospace());
        Observe(nameof(MainViewModel.MoistureAnalysisText), () => moisture.Value = _vm.MoistureAnalysisText);

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 2 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60f));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 55f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 45f));
        layout.Controls.Add(CreateGroup("Температурный профиль по толщине стены T(x)", profileChart), 0, 0);
        layout.Controls.Add(CreateGroup("Вклад слоев в теплосопротивление R", resistanceChart), 1, 0);
        layout.Controls.Add(moisture, 0, 1);
        layout.SetColumnSpan(moisture, 2);
        return layout;
    }

    // ---------- Правая панель: результаты ----------

    private Control BuildResultsPanel()
    {
        var results = new ResultsGroup();
        results.Bind(_vm);

        var advice = new TextOutputGroup("Нормы СН 2.04.01-2020 (РБ)", UiStyle.Regular())
        {
            Dock = DockStyle.None,
            Width = 300,
            Height = 280,
        };
        Observe(nameof(MainViewModel.AdviceText), () => advice.Value = _vm.AdviceText);

        var export = new Button
        {
            Text = "📄 Сформировать отчёт (РБ)",
            Width = 300,
            Height = 40,
            BackColor = Color.ForestGreen,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = UiStyle.Bold(),
            Margin = new Padding(0, 15, 0, 0),
            Command = _vm.ExportReportCommand,
        };

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
        };
        panel.Controls.Add(results);
        panel.Controls.Add(advice);
        panel.Controls.Add(export);
        return panel;
    }

    // ---------- Помощники ----------

    private static GroupBox CreateGroup(string title, Control content)
    {
        var group = new GroupBox { Text = title, Dock = DockStyle.Fill, Font = UiStyle.Bold() };
        group.Controls.Add(content);
        return group;
    }

    /// <summary>Сразу применяет значение свойства модели и обновляет его при каждом изменении.</summary>
    private void Observe(string propertyName, Action apply)
    {
        apply();
        _vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == propertyName)
                apply();
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _toolTip.Dispose();

        base.Dispose(disposing);
    }
}
