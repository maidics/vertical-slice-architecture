using VsaTemplate.Pipeline.Benchmarks.Enums;
using VsaTemplate.Pipeline.Benchmarks.Models;
using VsaTemplate.Pipeline.Benchmarks.Services;

namespace VsaTemplate.Pipeline.Benchmarks.BenchmarkCases;

public static class Instantiations
{
    public static TemplateBenchmarkCase Sequential =>
        new TemplateBenchmarkCaseBuilder($"{nameof(Instantiations)}.{nameof(Sequential)}", 10)
            .WithStep(TemplateBenchmarkStepType.Instantiate, 1)
            .Build();

    public static TemplateBenchmarkCase ConcurrentUncapped =>
        new TemplateBenchmarkCaseBuilder(
            $"{nameof(Instantiations)}.{nameof(ConcurrentUncapped)}",
            10
        )
            .WithStep(TemplateBenchmarkStepType.Instantiate, Environment.ProcessorCount)
            .Build();

    public static TemplateBenchmarkCase ConcurrentHalfCapped =>
        new TemplateBenchmarkCaseBuilder(
            $"{nameof(Instantiations)}.{nameof(ConcurrentHalfCapped)}",
            10
        )
            .WithStep(TemplateBenchmarkStepType.Instantiate, Environment.ProcessorCount / 2)
            .Build();
}
