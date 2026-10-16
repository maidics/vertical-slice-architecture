using VsaTemplate.Pipeline.Benchmarks.Enums;
using VsaTemplate.Pipeline.Benchmarks.Models;
using VsaTemplate.Pipeline.Benchmarks.Services;

namespace VsaTemplate.Pipeline.Benchmarks.BenchmarkCases;

// Note: running dotnet test picks up global.json where it is being run - must cd into tested solution's folder
// Note: a large chunk of tests will be spent to await Aspire to provision the DB => use in-memory DB for all tests except DB specific variants?
public static class Tests // add special case: build sln filter with all tests?
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
