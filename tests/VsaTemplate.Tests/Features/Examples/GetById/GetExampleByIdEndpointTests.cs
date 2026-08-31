using VsaTemplate.Features.Examples;
using VsaTemplate.Features.Examples.GetById;
using VsaTemplate.Tests.TestInfrastructure.WebTests;

namespace VsaTemplate.Tests.Features.Examples.GetById;

public sealed class GetExampleByIdEndpointTests : EndpointTestBase<GetExampleByIdEndpoint>
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
