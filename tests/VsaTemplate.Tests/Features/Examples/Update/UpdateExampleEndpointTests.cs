using VsaTemplate.Features.Examples;
using VsaTemplate.Features.Examples.Update;
using VsaTemplate.Tests.TestInfrastructure.WebTests;

namespace VsaTemplate.Tests.Features.Examples.Update;

public sealed class UpdateExampleEndpointTests : EndpointTestBase<UpdateExampleEndpoint>
{
    [Test]
    public override void ShouldHaveCorrectPrefix()
    {
        Prefix.ShouldBe(nameof(Example).ToLower());
    }

    [Test]
    public override void ShouldHaveCorrectTags()
    {
        Tags.ShouldBeEquivalentTo(Array.Empty<string>());
    }
}
