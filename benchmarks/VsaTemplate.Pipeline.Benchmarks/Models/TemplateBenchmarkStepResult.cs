using VsaTemplate.Pipeline.Benchmarks.Extensions;

namespace VsaTemplate.Pipeline.Benchmarks.Models;

public sealed record TemplateBenchmarkStepResult(
    string Name,
    TimeSpan ExecutionTime,
    long PeakMemory,
    long DiskSpace,
    List<TemplateBenchmarkCommandResult> CommandResults
)
{
    public override string ToString()
    {
        return $"""
                Name: {Name}
                    - Execution Time: {ExecutionTime.ToReadable()}
                    - Peak Memory: {PeakMemory.ToReadableBytes()}
                    - Disk Space: {DiskSpace.ToReadableBytes()}
                    - {string.Join("\n", CommandResults.Select(r => r.ToString()))}
            """;
    }
}
