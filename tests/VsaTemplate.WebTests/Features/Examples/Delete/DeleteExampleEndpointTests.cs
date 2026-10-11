using System.Net;
using Microsoft.EntityFrameworkCore;
using VsaTemplate.Domain.Constants;
using VsaTemplate.Domain.Entities;
using VsaTemplate.Features.Examples;

namespace VsaTemplate.WebTests.Features.Examples.Delete;

public sealed class DeleteExampleEndpointTests : EndpointTestBase<DeleteExampleEndpoint>
{
    protected override string Endpoint => "api/examples";

    [Test]
    public async Task ShouldReturnUnauthorizedWhenAnonymous()
    {
        using var client = CreateHttpClient();

        var response = await client.DeleteAsync(Endpoint + $"/{Guid.Empty}");
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task ShouldReturnForbiddenIfUserDoesNotHaveRequiredRole()
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
