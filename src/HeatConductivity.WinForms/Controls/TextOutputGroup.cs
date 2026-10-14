namespace HeatConductivity.WinForms.Controls;

/// <summary>Блок с заголовком и текстом только для чтения (вывод по норме, влажностный разбор).</summary>
internal sealed class TextOutputGroup : GroupBox
{
    private readonly TextBox _box;

    public TextOutputGroup(string title, Font textFont)
    {
        Text = title;
        Font = UiStyle.Bold();
        Dock = DockStyle.Fill;

        _box = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor = Color.White,
            Font = textFont,
        };
        Controls.Add(_box);
    }

    public string Value
    {
        get => _box.Text;
        set => _box.Text = value;
    }
}
