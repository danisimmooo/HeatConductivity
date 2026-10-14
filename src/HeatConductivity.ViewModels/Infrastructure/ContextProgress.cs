namespace HeatConductivity.ViewModels.Infrastructure;

/// <summary>
/// IProgress, который доставляет отчёты в заданный контекст синхронизации по порядку.
/// Без контекста (например, в тестах) вызывает обработчик сразу, в потоке отправителя.
/// </summary>
public sealed class ContextProgress<T> : IProgress<T>
{
    private readonly SynchronizationContext? _context;
    private readonly Action<T> _handler;

    public ContextProgress(SynchronizationContext? context, Action<T> handler)
    {
        _context = context;
        _handler = handler;
    }

    public void Report(T value)
    {
        if (_context == null)
            _handler(value);
        else
            _context.Post(_ => _handler(value), null);
    }
}
