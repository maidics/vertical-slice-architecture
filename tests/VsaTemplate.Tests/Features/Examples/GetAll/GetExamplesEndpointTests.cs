using VsaTemplate.Features.Examples;
using VsaTemplate.Features.Examples.GetAll;
using VsaTemplate.Tests.TestInfrastructure.WebTests;

namespace VsaTemplate.Tests.Features.Examples.GetAll;

public sealed class GetExamplesEndpointTests : EndpointTestBase<GetExamplesEndpoint>
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
