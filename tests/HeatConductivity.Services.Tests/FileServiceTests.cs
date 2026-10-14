using System.Text;
using HeatConductivity.Services.Infrastructure;
using Xunit;

namespace HeatConductivity.Services.Tests;

public class FileServiceTests
{
    [Fact]
    public void WriteAllText_writes_utf8_with_bom_like_the_original()
    {
        string path = Path.Combine(Path.GetTempPath(), $"hc-report-{Guid.NewGuid():N}.txt");
        try
        {
            new FileService().WriteAllText(path, "Отчёт: 5.764 м²·К/Вт");

            var bytes = File.ReadAllBytes(path);
            Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, bytes.Take(3));
            Assert.Equal("Отчёт: 5.764 м²·К/Вт", File.ReadAllText(path, Encoding.UTF8));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
