using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using VsaTemplate.Domain.Constants;
using VsaTemplate.Domain.Entities;
using VsaTemplate.Features.Examples;
using VsaTemplate.Tests.TestInfrastructure;
using VsaTemplate.Tests.TestInfrastructure.WebTests;

namespace VsaTemplate.Tests.Features.Examples.Create;

public sealed class CreateExampleEndpointTests : EndpointTestBase<CreateExampleEndpoint>
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

        CreateExampleEndpoint.Map(spy);

        var endpoints = spy.GetEndpoints();
        endpoints.Count.ShouldBe(1);

        var metadata = endpoints[0].Metadata;
        metadata.ShouldHaveEndpointName("CreateExample");
        metadata.ShouldHaveOneAuthMetadataWithRoles(Roles.User, Roles.Administrator);
    }

    [Test]
    public async Task ShouldReturnUnauthorizedWhenAnonymous()
    {
        var command = new CreateExampleCommand(string.Empty);

        using var client = CreateHttpClient();

        var response = await client.PostAsJsonAsync(Endpoint, command);
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task ShouldReturnForbiddenIfUserDoesNotHaveRequiredRole()
    {
        var command = new CreateExampleCommand(string.Empty);

        using var client = await LogInAsync();

        var response = await client.PostAsJsonAsync(Endpoint, command);
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task ShouldReturnBadRequestIfContentIsEmpty()
    {
        var command = new CreateExampleCommand(string.Empty);

        using var client = await LogInAsync(Roles.User);

        var response = await client.PostAsJsonAsync(Endpoint, command);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var validationProblem = await response.GetValidationProblemDetailsAsync();
        validationProblem.ShouldNotBeNull();
        validationProblem
            .Errors.TryGetValue(nameof(Example.Content), out var errors)
            .ShouldBeTrue();

        errors.ShouldNotBeNull();
        errors.Length.ShouldBe(1);
        errors.ShouldContain($"'{nameof(Example.Content)}' must not be empty.");
    }

    [Test]
    public async Task ShouldReturnConflictIfExampleWithContentAlreadyExists()
    {
        var example = new Example { Content = "test" };

        await SeedAsync(example);

        var command = new CreateExampleCommand(example.Content);

        using var client = await LogInAsync(Roles.User);

        var response = await client.PostAsJsonAsync(Endpoint, command);
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var errors = await response.GetResultErrorsAsync();
        errors.ShouldNotBeNull();
        errors.Length.ShouldBe(1);
        errors.ShouldContain($"{nameof(Example)} already exists with content: {command.Content}");
    }

    [Test]
    [Arguments(Roles.User)]
    [Arguments(Roles.Administrator)]
    [Arguments(Roles.User, Roles.Administrator)]
    public async Task ShouldReturnOkWithId(params string[] roles)
    {
        var command = new CreateExampleCommand("test");

        using var client = await LogInAsync(roles);

        var response = await client.PostAsJsonAsync(Endpoint, command);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var id = await response.Content.ReadFromJsonAsync<Guid>();
        id.ShouldNotBe(Guid.Empty);

        var created = await QueryAsync(c => c.Examples.FirstOrDefaultAsync(e => e.Id == id));
        created.ShouldNotBeNull();
    }
}
