using HeatConductivity.Services.Infrastructure;
using HeatConductivity.Services.Materials;
using HeatConductivity.Services.Presets;
using HeatConductivity.ViewModels;
using HeatConductivity.WinForms.Services;

namespace HeatConductivity.WinForms;

internal static class Program
{
    /// <summary>Точка входа: здесь собираются зависимости, больше нигде в приложении нет «new» сервисов.</summary>
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var viewModel = new MainViewModel(
            new InMemoryMaterialRepository(),
            new InMemoryWallPresetRepository(),
            new InMemoryClimatePresetRepository(),
            new WinFormsDialogService(),
            new FileService());

        Application.Run(new MainForm(viewModel));
    }
}
