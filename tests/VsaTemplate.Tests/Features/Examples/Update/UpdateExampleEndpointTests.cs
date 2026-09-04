using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using VsaTemplate.Domain.Constants;
using VsaTemplate.Domain.Entities;
using VsaTemplate.Features.Examples;
using VsaTemplate.Tests.TestInfrastructure.WebTests;

namespace VsaTemplate.Tests.Features.Examples.Update;

public sealed class UpdateExampleEndpointTests : EndpointTestBase<UpdateExampleEndpoint>
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

        UpdateExampleEndpoint.Map(spy);

        var endpoints = spy.GetEndpoints();
        endpoints.Count.ShouldBe(1);

        var metadata = endpoints[0].Metadata;
        metadata.ShouldHaveEndpointName("UpdateExample");
        metadata.ShouldHaveOneAuthMetadataWithRoles(Roles.User, Roles.Administrator);
    }

    [Test]
    public async Task ShouldReturnUnauthorizedWhenAnonymous()
    {
        var command = new UpdateExampleCommand(Guid.Empty, string.Empty);

        using var client = CreateHttpClient();

        var response = await client.PutAsJsonAsync(Endpoint, command);
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task ShouldReturnForbiddenIfUserDoesNotHaveRequiredRole()
    {
        var command = new UpdateExampleCommand(Guid.Empty, string.Empty);

        using var client = await LogInAsync();

        var response = await client.PutAsJsonAsync(Endpoint, command);
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task ShouldReturnBadRequestIfContentIsEmpty()
    {
        var command = new UpdateExampleCommand(Guid.Empty, string.Empty);

        using var client = await LogInAsync(Roles.User);

        var response = await client.PutAsJsonAsync(Endpoint, command);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var validationProblem = await response.GetValidationProblemDetailsAsync();
        validationProblem.ShouldNotBeNull();
        validationProblem.Errors.Count.ShouldBe(1);
        validationProblem
            .Errors.TryGetValue($"{nameof(Example.Content)}", out var errors)
            .ShouldBeTrue();

        errors.ShouldNotBeNull();
        errors.Length.ShouldBe(1);
        errors.ShouldContain($"'{nameof(Example.Content)}' must not be empty.");
    }

    [Test]
    public async Task ShouldReturnNotFoundWhenExampleDoesNotExist()
    {
        var command = new UpdateExampleCommand(Guid.Empty, "test");

        using var client = await LogInAsync(Roles.User);

        var response = await client.PutAsJsonAsync(Endpoint, command);
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var errors = await response.GetResultErrorsAsync();
        errors.ShouldNotBeNull();
        errors.Length.ShouldBe(1);
        errors.ShouldContain($"{nameof(Example)} not found.");
    }

    [Test]
    public async Task ShouldReturnConflictWhenExampleAlreadyExistsWithNewContent()
    {
        var example1 = new Example { Content = "test" };
        var example2 = new Example { Content = "" };

        await SeedAsync(example1, example2);

        var command = new UpdateExampleCommand(example2.Id, example1.Content);

        using var client = await LogInAsync(Roles.User);

        var response = await client.PutAsJsonAsync(Endpoint, command);
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var errors = await response.GetResultErrorsAsync();
        errors.ShouldNotBeNull();
        errors.Length.ShouldBe(1);
        errors.ShouldContain($"{nameof(Example)} with '{command.Content}' content already exists.");
    }

    [Test]
    [Arguments(Roles.User)]
    [Arguments(Roles.Administrator)]
    [Arguments(Roles.User, Roles.Administrator)]
    public async Task ShouldReturnNoContentWhenExampleContentHasBeenUpdated(params string[] roles)
    {
        var example = new Example { Content = "test" };

        await SeedAsync(example);

        var command = new UpdateExampleCommand(example.Id, "new-content");

        using var client = await LogInAsync(roles);

        var response = await client.PutAsJsonAsync(Endpoint, command);
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var updated = await QueryAsync(c =>
            c.Examples.FirstOrDefaultAsync(e => e.Id == example.Id)
        );
        updated.ShouldNotBeNull();
        updated.Content.ShouldBe(command.Content);
    }
}
