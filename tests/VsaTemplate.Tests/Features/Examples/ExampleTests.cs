using VsaTemplate.Domain.Entities;
using VsaTemplate.Features.Examples;

namespace VsaTemplate.Tests.Features.Examples;

public sealed class ExampleFeatureTests
{
    [Test]
    public void ShouldAppendContent()
    {
        const string content = "content";
        const string extra = "-extra-content";

        var example = new Example { Content = content };

        example.AppendContent(extra);

        example.Content.ShouldBe(content + extra);

        example.DomainEvents.Count.ShouldBe(1);
    }
}
