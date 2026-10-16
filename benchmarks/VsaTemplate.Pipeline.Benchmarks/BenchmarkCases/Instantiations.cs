using VsaTemplate.Pipeline.Benchmarks.Enums;
using VsaTemplate.Pipeline.Benchmarks.Models;
using VsaTemplate.Pipeline.Benchmarks.Services;

namespace VsaTemplate.Pipeline.Benchmarks.BenchmarkCases;

public static class Instantiations
{
    private static TemplateBenchmarkCaseBuilder CreateBuilder(string name) =>
        new TemplateBenchmarkCaseBuilder(10).WithName($"{nameof(Instantiations)}.{name}");

    public static TemplateBenchmarkCase Sequential =>
        CreateBuilder(nameof(Sequential))
            .WithStep(TemplateBenchmarkStepType.Instantiate, 1)
            .Build();

    public static TemplateBenchmarkCase ConcurrentUncapped =>
        CreateBuilder(nameof(ConcurrentUncapped))
            .WithStep(TemplateBenchmarkStepType.Instantiate, int.MaxValue) // dispatches all tasks at once
            .Build();

    public static TemplateBenchmarkCase ConcurrentCappedToCpuCount =>
        CreateBuilder(nameof(ConcurrentCappedToCpuCount))
            .WithStep(TemplateBenchmarkStepType.Instantiate, TemplateBenchmarkContainer.Host.MaxCpu) // dispatches CPU count amount
            .Build();

    public static TemplateBenchmarkCase ConcurrentCappedToHalfCpuCount =>
        CreateBuilder(nameof(ConcurrentCappedToHalfCpuCount))
            .WithStep(
                TemplateBenchmarkStepType.Instantiate,
                TemplateBenchmarkContainer.Host.MaxCpu / 2
            )
            .Build();
}
