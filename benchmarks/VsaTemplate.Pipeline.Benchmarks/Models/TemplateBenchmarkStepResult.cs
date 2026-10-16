namespace VsaTemplate.Pipeline.Benchmarks.Models;

public sealed record TemplateBenchmarkStepResult(
    string Name,
    TimeSpan ExecutionTime,
    long PeakMemory,
    long DiskSpace,
    List<TemplateBenchmarkCommandResult> CommandResults
);
