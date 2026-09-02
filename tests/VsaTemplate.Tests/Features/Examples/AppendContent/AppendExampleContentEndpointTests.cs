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
    public void MapMethodShouldMapEndpointWithAttributes()
    {
        var spy = new EndpointRouteBuilderSpy();

        AppendExampleContentEndpoint.Map(spy);

        var endpoints = spy.GetEndpoints();
        endpoints.Count.ShouldBe(1);

        var metadata = endpoints[0].Metadata;

        var name = metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName;
        name.ShouldNotBeNull();
        name.ShouldBe("AppendExampleContent");

        var authMetadata = metadata.GetOrderedMetadata<IAuthorizeData>();
        authMetadata.Count.ShouldBe(1);
        authMetadata[0].Roles.ShouldBe(string.Join(",", Roles.User, Roles.Administrator));
    }

    [Test]
    public async Task ShouldReturnUnauthorizedWhenAnonymous()
    {
        var command = new AppendExampleContentCommand(Guid.Empty, string.Empty);

        using var client = CreateHttpClient();

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

        using var client = CreateHttpClient();

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

        await using var context = CreateDbContext();

        await context.Examples.AddAsync(example2);
        await context.Examples.AddAsync(example1);
        await context.SaveChangesAsync();

        using var client = CreateHttpClient();

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
    public async Task ShouldReturnNoContentAndIdIfContentHasBeenAppended()
    {
        var example = new Example { Content = "test" };

        await using var context = CreateDbContext();

        await context.Examples.AddAsync(example);
        await context.SaveChangesAsync();

        using var client = CreateHttpClient();

        var command = new AppendExampleContentCommand(example.Id, "-content");

        var response = await client.PatchAsJsonAsync(Endpoint, command);
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        context.ChangeTracker.Clear();

        var updated = await context.Examples.FirstOrDefaultAsync(e => e.Id == example.Id);
        updated.ShouldNotBeNull();
        updated.Content.ShouldBe(example.Content + command.AdditionalContent);
        updated.HasAppendedContent.ShouldBeTrue();
    }
}
