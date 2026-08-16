using Microsoft.Extensions.Logging;

namespace VsaTemplate.TemplateTests.Infrastructure;

public sealed class LoggerSpy<T> : ILogger<T>
{
    private readonly List<(LogLevel Level, string Message)> _entries = new();

    public IReadOnlyList<(LogLevel Level, string Message)> Entries => _entries.AsReadOnly();

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null!;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    ) => _entries.Add((logLevel, formatter(state, exception)));
}
