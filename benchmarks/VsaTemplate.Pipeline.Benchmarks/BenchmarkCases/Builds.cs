using VsaTemplate.Pipeline.Benchmarks.Enums;
using VsaTemplate.Pipeline.Benchmarks.Models;
using VsaTemplate.Pipeline.Benchmarks.Services;

namespace VsaTemplate.Pipeline.Benchmarks.BenchmarkCases;

public static class Builds
{
    private static TemplateBenchmarkCaseBuilder CreateBuilder(string name) =>
        new TemplateBenchmarkCaseBuilder(10)
            .WithName($"{nameof(Restorations)}.{name}")
            .WithStep(TemplateBenchmarkStepType.Instantiate, TemplateBenchmarkContainer.Host.MaxCpu)
            .WithStep(TemplateBenchmarkStepType.CloneAndRestoreRepo, 1)
            .WithStep(
                TemplateBenchmarkStepType.Restore,
                TemplateBenchmarkContainer.Host.MaxCpu / 2
            );

    public static TemplateBenchmarkCase Sequential =>
        CreateBuilder(nameof(Sequential)).WithStep(TemplateBenchmarkStepType.Build, 1).Build();

    public static TemplateBenchmarkCase ConcurrentUncapped =>
        CreateBuilder(nameof(Sequential))
            .WithStep(TemplateBenchmarkStepType.Build, int.MaxValue)
            .Build();

    public static TemplateBenchmarkCase ConcurrentCappedToCpuCount =>
        CreateBuilder(nameof(Sequential))
            .WithStep(TemplateBenchmarkStepType.Build, TemplateBenchmarkContainer.Host.MaxCpu)
            .Build();

    public static TemplateBenchmarkCase ConcurrentCappedToHalfCpuCount =>
        CreateBuilder(nameof(Sequential))
            .WithStep(TemplateBenchmarkStepType.Build, TemplateBenchmarkContainer.Host.MaxCpu / 2)
            .Build();
}
