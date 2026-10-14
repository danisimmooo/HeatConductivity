using HeatConductivity.Core.Models;
using HeatConductivity.ViewModels;

namespace HeatConductivity.WinForms.Controls;

/// <summary>Конструкция стены: шаблон, таблица слоёв, кнопки добавления и удаления.</summary>
internal sealed class LayersGroup : GroupBox
{
    /// <summary>Строка выпадающего списка материалов: подпись и сам материал как значение ячейки.</summary>
    private sealed record MaterialOption(string Name, Material Material);

    private readonly ComboBox _presets;
    private readonly DataGridView _grid;
    private readonly DataGridViewComboBoxColumn _materialColumn;
    private readonly Button _addButton;
    private readonly Button _removeButton;

    public LayersGroup(ToolTip toolTip)
    {
        Text = "Конструкция стены (Слои)";
        Width = 350;
        Height = 310;
        Font = UiStyle.Bold();

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35f));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35f));

        // Шаблон
        var presetsPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
        presetsPanel.Controls.Add(new Label
        {
            Text = "Шаблон:",
            AutoSize = true,
            Margin = new Padding(0, 6, 5, 0),
            Font = UiStyle.Regular(),
        });
        _presets = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 250, Font = UiStyle.Regular() };
        toolTip.SetToolTip(_presets, "Типовые белорусские фасадные системы для соблюдения требований R ≥ 3.20 м²·К/Вт.");
        presetsPanel.Controls.Add(_presets);
        layout.Controls.Add(presetsPanel, 0, 0);

        // Таблица слоёв
        _materialColumn = new DataGridViewComboBoxColumn
        {
            HeaderText = "Материал",
            DataPropertyName = nameof(LayerViewModel.Material),
            DisplayMember = nameof(MaterialOption.Name),
            ValueMember = nameof(MaterialOption.Material),
            ValueType = typeof(Material),
        };
        var thicknessColumn = new DataGridViewTextBoxColumn
        {
            HeaderText = "Толщина, мм",
            DataPropertyName = nameof(LayerViewModel.ThicknessMm),
        };

        _grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            RowHeadersVisible = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            Font = UiStyle.Regular(),
        };
        _grid.Columns.Add(_materialColumn);
        _grid.Columns.Add(thicknessColumn);

        // Выбор в выпадающем списке применяется сразу, а не при уходе из ячейки.
        _grid.CurrentCellDirtyStateChanged += (_, _) =>
        {
            if (_grid.IsCurrentCellDirty)
                _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
        };

        // Неверный ввод (например, буквы в толщине) просто откатывается, без окна с исключением.
        _grid.DataError += (_, e) => e.ThrowException = false;

        layout.Controls.Add(_grid, 0, 1);

        // Кнопки
        var buttonsPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
        _addButton = new Button { Text = "➕ Слой", Width = 110, Height = 28, Font = UiStyle.Regular() };
        _removeButton = new Button { Text = "❌ Удалить", Width = 110, Height = 28, Font = UiStyle.Regular() };
        buttonsPanel.Controls.Add(_addButton);
        buttonsPanel.Controls.Add(_removeButton);
        layout.Controls.Add(buttonsPanel, 0, 2);

        Controls.Add(layout);
    }

    public void Bind(MainViewModel vm)
    {
        _presets.Items.AddRange(vm.WallPresetItems.Cast<object>().ToArray());
        _presets.DataBindings.Add(
            nameof(ComboBox.SelectedIndex), vm, nameof(MainViewModel.SelectedWallPresetIndex),
            true, DataSourceUpdateMode.OnPropertyChanged);

        _materialColumn.DataSource = vm.AvailableMaterials.Select(m => new MaterialOption(m.Name, m)).ToList();
        _grid.DataSource = vm.Layers;

        // Какой слой выбран в таблице, тот и удаляет кнопка «Удалить».
        _grid.SelectionChanged += (_, _) => SyncSelectedLayer(vm);
        vm.PropertyChanged += (_, e) =>
        {
            // Модель сбросила выбор (после удаления или смены шаблона) — берём то, что выделено в таблице.
            if (e.PropertyName == nameof(MainViewModel.SelectedLayer) && vm.SelectedLayer == null)
                SyncSelectedLayer(vm);
        };

        _addButton.Command = vm.AddLayerCommand;
        _removeButton.Command = vm.RemoveLayerCommand;

        DataBindings.Add(nameof(Enabled), vm, nameof(MainViewModel.IsEditingEnabled), false, DataSourceUpdateMode.Never);
    }

    private void SyncSelectedLayer(MainViewModel vm) =>
        vm.SelectedLayer = _grid.CurrentRow?.DataBoundItem as LayerViewModel;
}
