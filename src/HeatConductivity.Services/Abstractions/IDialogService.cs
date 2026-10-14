namespace HeatConductivity.Services.Abstractions;

/// <summary>Диалоги, которые нужны логике, но реализуются в UI-слое.</summary>
public interface IDialogService
{
    /// <summary>Показывает диалог сохранения. Возвращает путь или null, если пользователь отказался.</summary>
    string? PromptSaveFile(string suggestedFileName, string filter);

    void ShowInfo(string title, string message);
}
