using Microsoft.Testing.Platform.Logging;

namespace VsaTemplate.Tests.Infrastructure.TemplateTests;

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
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    ) => Entries.Add((logLevel, formatter(state, exception)));

    public bool IsEnabled(LogLevel logLevel) => true;
}
