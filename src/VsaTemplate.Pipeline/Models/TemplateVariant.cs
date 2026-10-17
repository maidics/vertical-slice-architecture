using VsaTemplate.Pipeline.Constants;

namespace VsaTemplate.Pipeline.Models;

public sealed record TemplateVariant
{
    public readonly string Name;
    public readonly IReadOnlyList<string> Arguments;
    public readonly IReadOnlyList<string> TestProjects;

    private TemplateVariant(string name, string[] arguments, string[] testProjects)
    {
        Name = name;
        Arguments = arguments;
        TestProjects = testProjects;
    }

    public static IReadOnlyList<TemplateVariant> All =>
        new List<TemplateVariant>
        {
            new("Default", [], [Template.ProjectPaths.UnitTests, Template.ProjectPaths.WebTests]),
            new(
                "ModularPipelines",
                [Options.ModularPipelines],
                [Template.ProjectPaths.UnitTests, Template.ProjectPaths.WebTests]
            ),
        }
            .SelectMany(variant =>
                new[]
                {
                    variant,
                    new TemplateVariant(
                        variant.Name + "WithExamples",
                        [.. variant.Arguments, Options.Examples],
                        [.. variant.TestProjects]
                    ),
                }
            )
            .ToList();

    private static class Options
    {
        public const string Examples = "-e";
        public const string ModularPipelines = "-mp";
    }
}
