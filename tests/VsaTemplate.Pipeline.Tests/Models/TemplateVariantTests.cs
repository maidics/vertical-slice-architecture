using Shouldly;
using VsaTemplate.Pipeline.Constants;
using VsaTemplate.Pipeline.Models;

namespace VsaTemplate.Pipeline.Tests.Models;

public sealed class TemplateVariantTests
{
    [Test]
    public void HalfOfAllTemplateVariantsShouldBeWithExamples()
    {
        var all = TemplateVariant.All;

        all.Count(v => v.Name.Contains("WithExamples") && v.Arguments.Contains("-e"))
            .ShouldBe(all.Count / 2);
    }

    [Test]
    public void SolutionAndTestProjectsShouldPointIntoVariantDirectory()
    {
        foreach (var variant in TemplateVariant.All)
        {
            var directory = Path.Combine(
                Template.ArtifactsDirectory.TemplateVariantsDirectory,
                variant.Name
            );

            variant.Solution.ShouldBe(Path.Combine(directory, $"{variant.Name}.slnx"));
            variant.TestProjects.ShouldBe([
                Path.Combine(
                    directory,
                    "tests",
                    $"{variant.Name}.UnitTests",
                    $"{variant.Name}.UnitTests.csproj"
                ),
                Path.Combine(
                    directory,
                    "tests",
                    $"{variant.Name}.WebTests",
                    $"{variant.Name}.WebTests.csproj"
                ),
            ]);
        }
    }
}
