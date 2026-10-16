using VsaTemplate.Pipeline.Benchmarks.Enums;

namespace VsaTemplate.Pipeline.Benchmarks.Models;

public sealed record TemplateBenchmarkStep
{
    public readonly string Name;
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
        MaxConcurrency = maxConcurrency;

        CommandLineArguments = GetCommandLineArguments(type, names).ToList();
    }

    public TemplateBenchmarkStep(
        string name,
        int maxConcurrency,
        List<List<string>> commandLineArguments
    )
    {
        Name = name;
        MaxConcurrency = maxConcurrency;
        CommandLineArguments = commandLineArguments;
    }

    private static IReadOnlyList<IReadOnlyList<string>> GetCommandLineArguments(
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
                    "--no-update-check",
                })
                .ToList(),
            TemplateBenchmarkStepType.CloneAndRestoreRepo =>
            [
                ["git", "clone", "https://github.com/maidics/vertical-slice-architecture"],
                ["dotnet", "restore", "vertical-slice-architecture/VsaTemplate.slnx"],
            ],
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
                // run from the solution folder: dotnet test only reads the Microsoft.Testing.Platform opt-in
                // from global.json in the working directory
                // exit code 8 = zero tests ran (IntegrationTests has no tests without the examples option)
                .Select(name => new List<string>
                {
                    "sh",
                    "-c",
                    $"cd {name} && dotnet test --solution {name}.slnx --no-restore --no-build --ignore-exit-code 8",
                })
                .ToList(),
            _ => throw new NotSupportedException(
                $"Unsupported {nameof(TemplateBenchmarkStepType)}: '{type}'."
            ),
        };
    }
}
