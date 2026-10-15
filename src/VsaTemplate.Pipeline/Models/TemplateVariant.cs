namespace VsaTemplate.Pipeline.Models;

public sealed record TemplateVariant
{
    public readonly string Name;
    public readonly IReadOnlyList<string> Arguments;

    private TemplateVariant(string name, string[] arguments)
    {
        Name = name;
        Arguments = arguments;
    }

    private static IReadOnlyList<TemplateVariant> All =>
        new List<TemplateVariant>
        {
            new("Default", []),
            new("ModularPipelines", [Options.ModularPipelines]),
        }
            .SelectMany(variant =>
                new[]
                {
                    variant,
                    new TemplateVariant(
                        variant.Name + "WithExamples",
                        [.. variant.Arguments, Options.Examples]
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
