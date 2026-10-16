using VsaTemplate.Pipeline.Benchmarks.Enums;
using VsaTemplate.Pipeline.Benchmarks.Models;
using VsaTemplate.Pipeline.Benchmarks.Services;

namespace VsaTemplate.Pipeline.Benchmarks.BenchmarkCases;

public static class Instantiations
{
    private static readonly TemplateBenchmarkCaseBuilder _builder = new(10);

    public static TemplateBenchmarkCase Sequential =>
        _builder
            .WithName($"{nameof(Instantiations)}.{nameof(Sequential)}")
            .WithStep(TemplateBenchmarkStepType.Instantiate, 1)
            .Build();

    public static TemplateBenchmarkCase ConcurrentUncapped =>
        _builder
            .WithName($"{nameof(Instantiations)}.{nameof(ConcurrentUncapped)}")
            .WithStep(TemplateBenchmarkStepType.Instantiate, int.MaxValue) // dispatches all tasks at once
            .Build();

    public static TemplateBenchmarkCase ConcurrentCappedToCpuCount =>
        _builder
            .WithName($"{nameof(Instantiations)}.{nameof(ConcurrentCappedToCpuCount)}")
            .WithStep(TemplateBenchmarkStepType.Instantiate, TemplateBenchmarkContainer.Host.MaxCpu) // dispatches CPU count amount
            .Build();

    public static TemplateBenchmarkCase ConcurrentCappedToHalfCpuCount =>
        _builder
            .WithName($"{nameof(Instantiations)}.{nameof(ConcurrentCappedToHalfCpuCount)}")
            .WithStep(
                TemplateBenchmarkStepType.Instantiate,
                TemplateBenchmarkContainer.Host.MaxCpu / 2
            )
            .Build();
}
