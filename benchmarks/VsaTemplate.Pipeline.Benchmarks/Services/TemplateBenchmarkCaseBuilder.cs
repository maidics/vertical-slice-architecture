using VsaTemplate.Pipeline.Benchmarks.Enums;
using VsaTemplate.Pipeline.Benchmarks.Models;

namespace VsaTemplate.Pipeline.Benchmarks.Services;

public sealed class TemplateBenchmarkCaseBuilder
{
    public readonly string Name;
    public readonly int InstantiationCount;

    private readonly IReadOnlyList<string> _names;

    public TemplateBenchmarkCaseBuilder(string name, int instantiationCount)
    {
        Name = name;
        InstantiationCount = instantiationCount;

        _names = Enumerable
            .Range(0, instantiationCount)
            .Select(_ => Guid.NewGuid().ToString())
            .ToList();
    }

    private readonly List<TemplateBenchmarkStep> _steps = [];

    public TemplateBenchmarkCaseBuilder WithStep(TemplateBenchmarkStepType type, int maxConcurrency)
    {
        _steps.Add(new TemplateBenchmarkStep(Name, type, _names, maxConcurrency));

        return this;
    }

    public TemplateBenchmarkCase Build()
    {
        return new TemplateBenchmarkCase(Name, _steps);
    }
}
