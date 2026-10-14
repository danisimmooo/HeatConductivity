using System.Windows.Input;

namespace HeatConductivity.ViewModels.Infrastructure;

/// <summary>
/// Асинхронная команда: пока выполняется, повторный запуск запрещён.
/// Исключения не проглатываются — они уходят в контекст синхронизации, как у обычного async-обработчика.
/// </summary>
public sealed class AsyncRelayCommand : ICommand
{
    private readonly Func<Task> _execute;
    private readonly Func<bool>? _canExecute;
    private bool _isExecuting;

    public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => !_isExecuting && (_canExecute?.Invoke() ?? true);

    public async void Execute(object? parameter) => await ExecuteAsync();

    /// <summary>Запуск с возможностью дождаться завершения (для тестов и вызова из кода).</summary>
    public async Task ExecuteAsync()
    {
        if (!CanExecute(null)) return;

        _isExecuting = true;
        RaiseCanExecuteChanged();
        try
        {
            await _execute();
        }
        finally
        {
            _isExecuting = false;
            RaiseCanExecuteChanged();
        }
    }

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
