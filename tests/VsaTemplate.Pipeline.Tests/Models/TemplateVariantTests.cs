using Shouldly;
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
}
