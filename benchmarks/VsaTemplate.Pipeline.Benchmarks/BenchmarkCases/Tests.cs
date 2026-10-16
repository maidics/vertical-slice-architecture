using VsaTemplate.Pipeline.Benchmarks.Enums;
using VsaTemplate.Pipeline.Benchmarks.Models;
using VsaTemplate.Pipeline.Benchmarks.Services;

namespace VsaTemplate.Pipeline.Benchmarks.BenchmarkCases;

public static class Tests
{
    private static TemplateBenchmarkCaseBuilder CreateBuilder(string name) =>
        new TemplateBenchmarkCaseBuilder(10)
            .WithName($"{nameof(Tests)}.{name}")
            .WithStep(TemplateBenchmarkStepType.Instantiate, TemplateBenchmarkContainer.Host.MaxCpu)
            .WithStep(TemplateBenchmarkStepType.CloneAndRestoreRepo, 1)
            .WithStep(TemplateBenchmarkStepType.Restore, 2)
            .WithStep(TemplateBenchmarkStepType.Build, 2);

    public static TemplateBenchmarkCase Sequential =>
        CreateBuilder(nameof(Sequential)).WithStep(TemplateBenchmarkStepType.Test, 1).Build();

    public static TemplateBenchmarkCase ConcurrentUncapped =>
        CreateBuilder(nameof(ConcurrentUncapped))
            .WithStep(TemplateBenchmarkStepType.Test, int.MaxValue)
            .Build();

    public static TemplateBenchmarkCase ConcurrentCappedToCpuCount =>
        CreateBuilder(nameof(ConcurrentCappedToCpuCount))
            .WithStep(TemplateBenchmarkStepType.Test, TemplateBenchmarkContainer.Host.MaxCpu)
            .Build();

    public static TemplateBenchmarkCase ConcurrentCappedToHalfCpuCount =>
        CreateBuilder(nameof(ConcurrentCappedToHalfCpuCount))
            .WithStep(TemplateBenchmarkStepType.Test, TemplateBenchmarkContainer.Host.MaxCpu / 2)
            .Build();
}
