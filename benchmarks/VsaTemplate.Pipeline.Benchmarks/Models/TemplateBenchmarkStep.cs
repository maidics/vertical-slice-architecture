using VsaTemplate.Pipeline.Benchmarks.Enums;

namespace VsaTemplate.Pipeline.Benchmarks.Models;

public sealed record TemplateBenchmarkStep
{
    public readonly string Name;
    public readonly TemplateBenchmarkStepType Type;
    public readonly int MaxConcurrency;

    public readonly IReadOnlyList<IReadOnlyList<string>> CommandLineArguments;

    public TemplateBenchmarkStep(
        string name,
        TemplateBenchmarkStepType type,
        IReadOnlyList<string> names,
        int maxConcurrency
    )
    {
        Name = $"{type}_{name}";
        Type = type;
        MaxConcurrency = maxConcurrency;

        CommandLineArguments = GetCommandLineArguments(type, names).ToList();
    }

    private static IReadOnlyList<IReadOnlyList<string>> GetCommandLineArguments( // TODO: add git clone repo and restore step
        TemplateBenchmarkStepType type,
        IReadOnlyList<string> names
    )
    {
        return type switch
        {
            TemplateBenchmarkStepType.Instantiate => names
                .Select(name => new List<string>
                {
                    "dotnet",
                    "new",
                    "vsa-sln",
                    "-n",
                    name,
                    "-o",
                    name,
                })
                .ToList(),
            TemplateBenchmarkStepType.Restore => names
                .Select(name => new List<string> { "dotnet", "restore", $"{name}/{name}.slnx" })
                .ToList(),
            TemplateBenchmarkStepType.Build => names
                .Select(name => new List<string>
                {
                    "dotnet",
                    "build",
                    $"{name}/{name}.slnx",
                    "--no-restore",
                })
                .ToList(),
            TemplateBenchmarkStepType.Test => names
                .Select(name => new List<string>
                {
                    "dotnet",
                    "test",
                    $"{name}/{name}.slnx",
                    "--no-restore",
                    "--no-build",
                })
                .ToList(),
            _ => throw new NotSupportedException(
                $"Unsupported {nameof(TemplateBenchmarkStepType)}: '{type}'."
            ),
        };
    }
}
