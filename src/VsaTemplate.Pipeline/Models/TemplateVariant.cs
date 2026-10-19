using VsaTemplate.Pipeline.Constants;

namespace VsaTemplate.Pipeline.Models;

public sealed record TemplateVariant
{
    public readonly string Name;
    public readonly IReadOnlyList<string> Arguments;
    public readonly string Solution;
    public readonly IReadOnlyList<string> TestProjects;

    // testProjects are project name suffixes: instantiating with '-n {name}' renames 'VsaTemplate.UnitTests' to '{name}.UnitTests'
    private TemplateVariant(string name, string[] arguments, string[] testProjects)
    {
        var directory = Path.Combine(Template.ArtifactsDirectory.TemplateVariantsDirectory, name);

        Name = name;
        Arguments = arguments;
        Solution = Path.Combine(directory, name + ".slnx");
        TestProjects =
        [
            .. testProjects.Select(p =>
                Path.Combine(directory, "tests", $"{name}.{p}", $"{name}.{p}.csproj")
            ),
        ];
    }

    public static IReadOnlyList<TemplateVariant> All =>
        new (string Name, string[] Arguments, string[] TestProjects)[]
        {
            ("Default", [], [Projects.UnitTests, Projects.WebTests]),
            ("ModularPipelines", [Options.ModularPipelines], [Projects.UnitTests, Projects.WebTests]),
        }
            .SelectMany(variant =>
                new[]
                {
                    new TemplateVariant(variant.Name, variant.Arguments, variant.TestProjects),
                    new TemplateVariant(
                        variant.Name + "WithExamples",
                        [.. variant.Arguments, Options.Examples],
                        variant.TestProjects
                    ),
                }
            )
            .ToList();

    private static class Projects
    {
        public const string UnitTests = "UnitTests";
        public const string WebTests = "WebTests";
    }

    private static class Options
    {
        public const string Examples = "-e";
        public const string ModularPipelines = "-mp";
    }
}
