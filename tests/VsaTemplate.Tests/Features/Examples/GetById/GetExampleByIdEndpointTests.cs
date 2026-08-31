using System.Net;
using System.Net.Http.Json;
using VsaTemplate.Features.Examples;
using VsaTemplate.Features.Examples.GetById;
using VsaTemplate.Tests.TestInfrastructure;
using VsaTemplate.Tests.TestInfrastructure.WebTests;

namespace VsaTemplate.Tests.Features.Examples.GetById;

public sealed class GetExampleByIdEndpointTests : EndpointTestBase<GetExampleByIdEndpoint>
{
    protected override string Endpoint => "api/examples";

    [Test]
    public override void ShouldHaveCorrectPrefix()
    {
        Prefix.ShouldBe("examples");
    }

    [Test]
    public override void ShouldHaveCorrectTags()
    {
        Tags.ShouldBeEquivalentTo(Array.Empty<string>());
    }

    [Test]
    public async Task ShouldReturnNotFoundIfExampleDoesNotExist()
    {
        using var client = CreateHttpClient();

        var response = await client.GetAsync(Endpoint + $"/{Guid.Empty}");
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var errors = await response.GetResultErrorsAsync();
        errors.ShouldNotBeNull();
        errors.Length.ShouldBe(1);
        errors.ShouldContain($"{nameof(Example)} not found.");
    }

    [Test]
    public async Task ShouldReturnOkAndExampleById()
    {
        var example = new Example { Content = "test" };

        await using var context = CreateDbContext();

        await context.Examples.AddAsync(example);
        await context.SaveChangesAsync();

        using var client = CreateHttpClient();

        var response = await client.GetAsync(Endpoint + $"/{example.Id}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var returned = await response.Content.ReadFromJsonAsync<Example>();
        returned.ShouldNotBeNull();
        returned.ShouldBeEquivalentTo(example);
    }
}
