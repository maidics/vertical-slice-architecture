using VsaTemplate.Pipeline.Benchmarks.Enums;
using VsaTemplate.Pipeline.Benchmarks.Models;
using VsaTemplate.Pipeline.Benchmarks.Services;

namespace VsaTemplate.Pipeline.Benchmarks.BenchmarkCases;

public static class Builds
{
    private static TemplateBenchmarkCaseBuilder CreateBuilder(string name) =>
        new TemplateBenchmarkCaseBuilder(10)
            .WithName($"{nameof(Builds)}.{name}")
            .WithStep(TemplateBenchmarkStepType.Instantiate, TemplateBenchmarkContainer.Host.MaxCpu)
            .WithStep(TemplateBenchmarkStepType.CloneAndRestoreRepo, 1)
            .WithStep(
                TemplateBenchmarkStepType.Restore,
                TemplateBenchmarkContainer.Host.MaxCpu / 2
            );

    public static TemplateBenchmarkCase Sequential =>
        CreateBuilder(nameof(Sequential)).WithStep(TemplateBenchmarkStepType.Build, 1).Build();

    public static TemplateBenchmarkCase ConcurrentUncapped =>
        CreateBuilder(nameof(ConcurrentUncapped))
            .WithStep(TemplateBenchmarkStepType.Build, int.MaxValue)
            .Build();

    public static TemplateBenchmarkCase ConcurrentCappedToCpuCount => // Winner: dotnet build uses the whole machine
        CreateBuilder(nameof(ConcurrentCappedToCpuCount)) // While one awaits something to finish the other can fill those gaps - reducing time
            .WithStep(TemplateBenchmarkStepType.Build, TemplateBenchmarkContainer.Host.MaxCpu) // this is a well-supported guess not something that is test
            .Build(); // using 2 instead of MaxCpu / 2 as maxConcurrency wins

    public static TemplateBenchmarkCase ConcurrentCappedToHalfCpuCount =>
        CreateBuilder(nameof(ConcurrentCappedToHalfCpuCount))
            .WithStep(TemplateBenchmarkStepType.Build, TemplateBenchmarkContainer.Host.MaxCpu / 2)
            .Build();
}
