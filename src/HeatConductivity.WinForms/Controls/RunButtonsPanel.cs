using HeatConductivity.ViewModels;

namespace HeatConductivity.WinForms.Controls;

/// <summary>Кнопки «Запустить» и «Стоп». Доступность каждой задаёт команда модели.</summary>
internal sealed class RunButtonsPanel : FlowLayoutPanel
{
    private readonly Button _start;
    private readonly Button _stop;

    public RunButtonsPanel()
    {
        Width = 350;
        Height = 50;
        Margin = new Padding(0, 10, 0, 0);

        _start = new Button
        {
            Text = "▶ Запустить динамику",
            Width = 165,
            Height = 38,
            BackColor = Color.DodgerBlue,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Font = UiStyle.Bold(),
        };
        _stop = new Button
        {
            Text = "⏹ Стоп",
            Width = 155,
            Height = 38,
            FlatStyle = FlatStyle.Flat,
            Font = UiStyle.Regular(9f),
        };

        Controls.Add(_start);
        Controls.Add(_stop);
    }

    public void Bind(MainViewModel vm)
    {
        _start.Command = vm.StartCommand;
        _stop.Command = vm.StopCommand;
    }
}
