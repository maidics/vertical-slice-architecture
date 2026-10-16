namespace VsaTemplate.Pipeline.Benchmarks.Models;

public sealed record TemplateBenchmarkCase
{
    public readonly string Name;
    public readonly IReadOnlyList<TemplateBenchmarkStep> Steps;

    public TemplateBenchmarkCase(string name, List<TemplateBenchmarkStep> steps)
    {
        Name = name;
        Steps = steps.ToList();
    }
}
