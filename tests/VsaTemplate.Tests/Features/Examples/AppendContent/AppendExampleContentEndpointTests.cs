using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using VsaTemplate.Domain.Constants;
using VsaTemplate.Domain.Entities;
using VsaTemplate.Features.Examples;
using VsaTemplate.Tests.TestInfrastructure;
using VsaTemplate.Tests.TestInfrastructure.WebTests;

namespace VsaTemplate.Tests.Features.Examples.AppendContent;

public sealed class AppendExampleContentEndpointTests
    : EndpointTestBase<AppendExampleContentEndpoint>
{
    protected override string Endpoint => "api/examples/append-content";

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

        AppendExampleContentEndpoint.Map(spy);

        var endpoints = spy.GetEndpoints();
        endpoints.Count.ShouldBe(1);

        var metadata = endpoints[0].Metadata;
        metadata.ShouldHaveEndpointName("AppendExampleContent");
        metadata.ShouldHaveOneAuthMetadataWithRoles(Roles.User, Roles.Administrator);
    }

    [Test]
    public async Task ShouldReturnUnauthorizedWhenAnonymous()
    {
        var command = new AppendExampleContentCommand(Guid.Empty, string.Empty);

        using var client = CreateHttpClient();

        var response = await client.PatchAsJsonAsync(Endpoint, command);
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task ShouldReturnForbiddenIfUserDoesNotHaveRequiredRole()
    {
        var command = new AppendExampleContentCommand(Guid.Empty, string.Empty);

        using var client = await LogInAsync();

        var response = await client.PatchAsJsonAsync(Endpoint, command);
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task ShouldReturnBadRequestIfAdditionalContentIsEmpty()
    {
        var command = new AppendExampleContentCommand(Guid.Empty, string.Empty);

        using var client = await LogInAsync(Roles.User);

        var response = await client.PatchAsJsonAsync(Endpoint, command);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var validationProblem = await response.GetValidationProblemDetailsAsync();
        validationProblem.ShouldNotBeNull();
        validationProblem
            .Errors.TryGetValue(
                nameof(AppendExampleContentCommand.AdditionalContent),
                out var errors
            )
            .ShouldBeTrue();

        errors.ShouldNotBeNull();
        errors.Length.ShouldBe(1);
        errors.ShouldContain("'Additional Content' must not be empty.");
    }

    [Test]
    public async Task ShouldReturnNotFoundIfExampleDoesNotExist()
    {
        var command = new AppendExampleContentCommand(Guid.Empty, "test");

        using var client = await LogInAsync(Roles.User);

        var response = await client.PatchAsJsonAsync(Endpoint, command);
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var errors = await response.GetResultErrorsAsync();
        errors.ShouldNotBeNull();
        errors.Length.ShouldBe(1);
        errors.ShouldContain($"{nameof(Example)} not found.");
    }

    [Test]
    public async Task ShouldReturnConflictIfExampleExistsWithContent()
    {
        var example1 = new Example { Content = "test-content" };
        var example2 = new Example { Content = "test" };

        await SeedAsync(example1, example2);

        using var client = await LogInAsync(Roles.User);

        var command = new AppendExampleContentCommand(example2.Id, "-content");

        var response = await client.PatchAsJsonAsync(Endpoint, command);
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var errors = await response.GetResultErrorsAsync();
        errors.ShouldNotBeNull();
        errors.Length.ShouldBe(1);
        errors.ShouldContain(
            $"{nameof(Example)} with '{example1.Content}' content already exists."
        );
    }

    [Test]
    [Arguments(Roles.User)]
    [Arguments(Roles.Administrator)]
    [Arguments(Roles.User, Roles.Administrator)]
    public async Task ShouldReturnNoContentAndIdIfContentHasBeenAppended(params string[] roles)
    {
        var example = new Example { Content = "test" };

        await SeedAsync(example);

        using var client = await LogInAsync(roles);

        var command = new AppendExampleContentCommand(example.Id, "-content");

        var response = await client.PatchAsJsonAsync(Endpoint, command);
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var updated = await QueryAsync(c =>
            c.Examples.FirstOrDefaultAsync(e => e.Id == example.Id)
        );
        updated.ShouldNotBeNull();
        updated.Content.ShouldBe(example.Content + command.AdditionalContent);
        updated.HasAppendedContent.ShouldBeTrue();
    }
}
