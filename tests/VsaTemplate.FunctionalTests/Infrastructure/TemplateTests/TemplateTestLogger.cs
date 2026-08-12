using Microsoft.Extensions.Logging;

namespace VsaTemplate.FunctionalTests.Infrastructure.TemplateTests;

public sealed class TemplateTestLogger<T> : ILogger<T>
{
    public readonly List<(LogLevel Level, string Message)> Entries = new();

    public Task LogAsync<TState>(
        LogLevel logLevel,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        Entries.Add((logLevel, formatter(state, exception)));

        return Task.CompletedTask;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    ) => Entries.Add((logLevel, formatter(state, exception)));

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull => null!;
}
