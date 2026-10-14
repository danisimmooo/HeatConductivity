using System.Text;
using HeatConductivity.Services.Abstractions;

namespace HeatConductivity.Services.Infrastructure;

/// <summary>Запись файлов в UTF-8 с BOM — как делала исходная программа (Encoding.UTF8).</summary>
public sealed class FileService : IFileService
{
    public void WriteAllText(string path, string content) =>
        File.WriteAllText(path, content, Encoding.UTF8);
}
