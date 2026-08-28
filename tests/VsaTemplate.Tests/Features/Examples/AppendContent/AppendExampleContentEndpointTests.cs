using VsaTemplate.Features.Examples;
using VsaTemplate.Features.Examples.AppendContent;
using VsaTemplate.Tests.TestInfrastructure.WebTests;

namespace VsaTemplate.Tests.Features.Examples.AppendContent;

public sealed class AppendExampleContentEndpointTests
    : EndpointTestBase<AppendExampleContentEndpoint>
{
    [Test]
    public override void ShouldHaveCorrectPrefix()
    {
        Prefix.ShouldBe(nameof(Example));
    }

    [Test]
    public override void ShouldHaveCorrectTags()
    {
        Tags.ShouldBeEquivalentTo(Array.Empty<string>());
    }
}
