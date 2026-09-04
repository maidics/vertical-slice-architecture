using System.Net;
using System.Net.Http.Json;
using VsaTemplate.Domain.Entities;
using VsaTemplate.Features.Examples;
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
    public override void MapMethodShouldMapEndpointWithAttributes()
    {
        var spy = CreateEndpointRouteBuilderSpy();

        GetExampleByIdEndpoint.Map(spy);

        var endpoints = spy.GetEndpoints();
        endpoints.Count.ShouldBe(1);

        var metadata = endpoints[0].Metadata;
        metadata.ShouldHaveEndpointName("GetExampleById");
        metadata.ShouldHaveOneAuthMetadataWithoutRoles();
    }

    [Test]
    public async Task ShouldReturnUnauthorizedWhenAnonymous()
    {
        using var client = CreateHttpClient();

        var response = await client.GetAsync(Endpoint + $"/{Guid.Empty}");
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfExampleDoesNotExist()
    {
        using var client = await LogInAsync();

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

        await SeedAsync(example);

        using var client = await LogInAsync();

        var response = await client.GetAsync(Endpoint + $"/{example.Id}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var returned = await response.Content.ReadFromJsonAsync<ExampleDto>();
        returned.ShouldNotBeNull();
        returned.Id.ShouldBe(example.Id);
    }
}
