using VsaTemplate.Pipeline.Benchmarks.Enums;
using VsaTemplate.Pipeline.Benchmarks.Models;
using VsaTemplate.Pipeline.Benchmarks.Services;

namespace VsaTemplate.Pipeline.Benchmarks.BenchmarkCases;

public static class Restorations
{
    private static TemplateBenchmarkCaseBuilder CreateBuilder(string name) =>
        new TemplateBenchmarkCaseBuilder(10)
            .WithName($"{nameof(Restorations)}.{name}")
            .WithStep(TemplateBenchmarkStepType.Instantiate, TemplateBenchmarkContainer.Host.MaxCpu);

    // previous findings show that the most optional instantiation method is
    // capping at host CPU count with --no-update-check
    // because of the commands form a transitive chain each command benchmark moves forward
    // with the previous' best result
    public static TemplateBenchmarkCase Sequential =>
        CreateBuilder(nameof(Sequential))
            .WithStep(TemplateBenchmarkStepType.Restore, 1)
            .Build();

    public static TemplateBenchmarkCase ConcurrentUncapped => // NuGet uses a global cache on the machine so these might fail without restoring all packages before running concurrent restore
        CreateBuilder(nameof(ConcurrentUncapped))
            .WithStep(TemplateBenchmarkStepType.Restore, int.MaxValue)
            .Build();

    public static TemplateBenchmarkCase ConcurrentCappedToCpuCount =>
        CreateBuilder(nameof(ConcurrentCappedToCpuCount))
            .WithStep(TemplateBenchmarkStepType.Restore, TemplateBenchmarkContainer.Host.MaxCpu)
            .Build();

    public static TemplateBenchmarkCase ConcurrentCappedToHalfCpuCount =>
        CreateBuilder(nameof(ConcurrentCappedToHalfCpuCount))
            .WithStep(TemplateBenchmarkStepType.Restore, TemplateBenchmarkContainer.Host.MaxCpu / 2)
            .Build();

    // ####################################################################################
    // ############################ Clone and restore repo cases ##########################
    // ####################################################################################

    public static TemplateBenchmarkCase SequentialWithCloneAndRestoreRepo =>
        CreateBuilder(nameof(SequentialWithCloneAndRestoreRepo))
            .WithStep(TemplateBenchmarkStepType.CloneAndRestoreRepo, 1) // concurrency does not matter here
            .WithStep(TemplateBenchmarkStepType.Restore, 1)
            .Build();

    public static TemplateBenchmarkCase ConcurrentUncappedWithCloneAndRestoreRepo => // NuGet uses a global cache on the machine so these might fail without restoring all packages before running concurrent restore
        CreateBuilder(nameof(ConcurrentUncappedWithCloneAndRestoreRepo))
            .WithStep(TemplateBenchmarkStepType.CloneAndRestoreRepo, 1)
            .WithStep(TemplateBenchmarkStepType.Restore, int.MaxValue)
            .Build();

    public static TemplateBenchmarkCase ConcurrentCappedToCpuCountWithCloneAndRestoreRepo =>
        CreateBuilder(nameof(ConcurrentCappedToCpuCountWithCloneAndRestoreRepo))
            .WithStep(TemplateBenchmarkStepType.CloneAndRestoreRepo, 1)
            .WithStep(TemplateBenchmarkStepType.Restore, TemplateBenchmarkContainer.Host.MaxCpu)
            .Build();

    public static TemplateBenchmarkCase ConcurrentCappedToHalfCpuCountWithCloneAndRestoreRepo =>
        CreateBuilder(nameof(ConcurrentCappedToHalfCpuCountWithCloneAndRestoreRepo))
            .WithStep(TemplateBenchmarkStepType.CloneAndRestoreRepo, 1)
            .WithStep(TemplateBenchmarkStepType.Restore, TemplateBenchmarkContainer.Host.MaxCpu / 2)
            .Build();
}
