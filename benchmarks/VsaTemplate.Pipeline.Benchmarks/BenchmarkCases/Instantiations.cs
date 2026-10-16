using VsaTemplate.Pipeline.Benchmarks.Enums;
using VsaTemplate.Pipeline.Benchmarks.Models;
using VsaTemplate.Pipeline.Benchmarks.Services;

namespace VsaTemplate.Pipeline.Benchmarks.BenchmarkCases;

public static class Instantiations
{
    public static TemplateBenchmarkCase Sequential =>
        new TemplateBenchmarkCaseBuilder($"{nameof(Instantiations)}.{nameof(Sequential)}", 10)
            .WithStep(TemplateBenchmarkStepType.Instantiate, 0)
            .Build();

    public static TemplateBenchmarkCase Concurrent =>
        new TemplateBenchmarkCaseBuilder($"{nameof(Instantiations)}.{nameof(Concurrent)}", 10)
            .WithStep(TemplateBenchmarkStepType.Instantiate, 10)
            .Build();
}
