using System.Net;
using Microsoft.EntityFrameworkCore;
using VsaTemplate.Domain.Constants;
using VsaTemplate.Domain.Entities;
using VsaTemplate.Features.Examples;
using VsaTemplate.Tests.TestInfrastructure;
using VsaTemplate.Tests.TestInfrastructure.WebTests;

namespace VsaTemplate.Tests.Features.Examples.Delete;

public sealed class DeleteExampleEndpointTests : EndpointTestBase<DeleteExampleEndpoint>
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

        DeleteExampleEndpoint.Map(spy);

        var endpoints = spy.GetEndpoints();
        endpoints.Count.ShouldBe(1);

        var metadata = endpoints[0].Metadata;
        metadata.ShouldHaveEndpointName("DeleteExample");
        metadata.ShouldHaveOneAuthMetadataWithRoles(Roles.Administrator);
    }

    [Test]
    public async Task ShouldReturnAuthorizedWhenAnonymous()
    {
        using var client = CreateHttpClient();

        var response = await client.DeleteAsync(Endpoint + $"/{Guid.Empty}");
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task ShouldReturnForbiddenForUserRole()
    {
        using var client = await LogInAsync(Roles.User);

        var response = await client.DeleteAsync(Endpoint + $"/{Guid.Empty}");
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task ShouldReturnNotFoundIfExampleNotExists()
    {
        using var client = await LogInAsync(Roles.Administrator);

        var response = await client.DeleteAsync(Endpoint + $"/{Guid.Empty}");
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var errors = await response.GetResultErrorsAsync();
        errors.ShouldNotBeNull();
        errors.Length.ShouldBe(1);
        errors.ShouldContain($"{nameof(Example)} not found.");
    }

    [Test]
    public async Task ShouldReturnNoContentIfExampleHasBeenDeleted()
    {
        var example = new Example { Content = "test" };

        await SeedAsync(example);

        using var client = await LogInAsync(Roles.Administrator);

        var response = await client.DeleteAsync(Endpoint + $"/{example.Id}");
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var deleted = await QueryAsync(c =>
            c.Examples.FirstOrDefaultAsync(e => e.Id == example.Id)
        );
        deleted.ShouldBeNull();
    }
}
