using VsaTemplate.Pipeline.Benchmarks.Enums;
using VsaTemplate.Pipeline.Benchmarks.Models;

namespace VsaTemplate.Pipeline.Benchmarks.Services;

public sealed class TemplateBenchmarkCaseBuilder
{
    public string Name = string.Empty;
    public readonly int InstantiationCount;

    private readonly IReadOnlyList<string> _names;

    public TemplateBenchmarkCaseBuilder(int instantiationCount)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(instantiationCount, 0);

        InstantiationCount = instantiationCount;

        _names = Enumerable
            .Range(0, instantiationCount)
            .Select(_ => Guid.NewGuid().ToString())
            .ToList();
    }

    private readonly List<TemplateBenchmarkStep> _steps = [];

    public TemplateBenchmarkCaseBuilder WithName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        Name = name;

        return this;
    }

    public TemplateBenchmarkCaseBuilder WithStep(TemplateBenchmarkStepType type, int maxConcurrency)
    {
        _steps.Add(new TemplateBenchmarkStep(Name, type, _names, maxConcurrency));

        return this;
    }

    public TemplateBenchmarkCase Build()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new InvalidOperationException(
                $"Name of {nameof(TemplateBenchmarkCaseBuilder)} is required."
            );

        if (_steps.Count == 0)
            throw new InvalidOperationException(
                $"At least one {nameof(TemplateBenchmarkStep)} is required."
            );

        return new TemplateBenchmarkCase(Name, _steps);
    }
}
