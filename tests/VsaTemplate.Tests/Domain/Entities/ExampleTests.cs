using VsaTemplate.Domain.Entities;

namespace VsaTemplate.Tests.Domain.Entities;

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
