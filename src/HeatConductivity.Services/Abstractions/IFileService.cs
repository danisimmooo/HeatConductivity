namespace HeatConductivity.Services.Abstractions;

public interface IFileService
{
    void WriteAllText(string path, string content);
}
