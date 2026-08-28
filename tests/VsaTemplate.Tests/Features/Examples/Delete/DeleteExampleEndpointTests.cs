using VsaTemplate.Features.Examples;
using VsaTemplate.Features.Examples.Delete;
using VsaTemplate.Tests.TestInfrastructure.WebTests;

namespace VsaTemplate.Tests.Features.Examples.Delete;

public sealed class DeleteExampleEndpointTests : EndpointTestBase<DeleteExampleEndpoint>
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
