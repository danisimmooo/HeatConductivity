using HeatConductivity.Services.Abstractions;

namespace HeatConductivity.WinForms.Services;

/// <summary>Стандартные диалоги Windows для логики, которая о них не знает.</summary>
internal sealed class WinFormsDialogService : IDialogService
{
    public string? PromptSaveFile(string suggestedFileName, string filter)
    {
        using var dialog = new SaveFileDialog
        {
            Filter = filter,
            FileName = suggestedFileName,
        };

        return dialog.ShowDialog(Form.ActiveForm) == DialogResult.OK ? dialog.FileName : null;
    }

    public void ShowInfo(string title, string message) =>
        MessageBox.Show(Form.ActiveForm, message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
}
