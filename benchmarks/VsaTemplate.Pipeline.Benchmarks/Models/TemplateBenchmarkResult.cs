namespace VsaTemplate.Pipeline.Benchmarks.Models;

public sealed record TemplateBenchmarkResult(
    string Name,
    IReadOnlyList<TemplateBenchmarkStepResult> StepResults
);
