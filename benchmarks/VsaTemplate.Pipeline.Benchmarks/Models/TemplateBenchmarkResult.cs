namespace VsaTemplate.Pipeline.Benchmarks.Models;

public sealed record TemplateBenchmarkResult(
    string Name,
    IReadOnlyList<TemplateBenchmarkStepResult> StepResults
)
{
    public override string ToString()
    {
        return $"""
               Name: {Name}
                    - Results: 
                        {string.Join("\n", StepResults.Select(r => r.ToString()))} 
            """;
    }
}
