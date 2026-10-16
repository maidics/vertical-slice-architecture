namespace VsaTemplate.Pipeline.Benchmarks.Models;

public sealed record TemplateBenchmarkCommandResult(
    string Command,
    long? ExitCode,
    string Stdout,
    string Stderr
);
