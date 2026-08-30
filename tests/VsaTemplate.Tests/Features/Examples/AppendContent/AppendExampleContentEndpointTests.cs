using System.Net;
using System.Net.Http.Json;
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

    [Test]
    public async Task ShouldReturnNotFoundIfExampleNotFound()
    {
        var command = new AppendExampleContentCommand(Guid.NewGuid(), "test");

        var result = await Client.PatchAsJsonAsync("api/example/append-content", command);

        result.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
