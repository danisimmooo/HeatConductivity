using HeatConductivity.Services.Abstractions;

namespace HeatConductivity.ViewModels.Tests;

internal sealed class FakeDialogService : IDialogService
{
    public string? PathToReturn { get; set; }
    public string? LastSuggestedName { get; private set; }
    public string? LastFilter { get; private set; }
    public List<(string Title, string Message)> Messages { get; } = new();

    public string? PromptSaveFile(string suggestedFileName, string filter)
    {
        LastSuggestedName = suggestedFileName;
        LastFilter = filter;
        return PathToReturn;
    }

    public void ShowInfo(string title, string message) => Messages.Add((title, message));
}

internal sealed class FakeFileService : IFileService
{
    public Dictionary<string, string> Written { get; } = new();
    public bool ThrowIoError { get; set; }

    public void WriteAllText(string path, string content)
    {
        if (ThrowIoError) throw new IOException("Диск недоступен");
        Written[path] = content;
    }
}
