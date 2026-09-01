using System.Net;
using System.Net.Http.Json;
using VsaTemplate.Features.Examples;
using VsaTemplate.Features.Examples.Update;
using VsaTemplate.Tests.TestInfrastructure;
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
    public async Task ShouldReturnBadRequestIfContentIsEmpty()
    {
        var command = new UpdateExampleCommand(Guid.Empty, string.Empty);

        using var client = CreateHttpClient();

        var response = await client.PutAsJsonAsync(Endpoint, command);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var validationProblem = await response.GetValidationProblemDetailsAsync();
        validationProblem.ShouldNotBeNull();
        validationProblem.Errors.Count.ShouldBe(1);
        validationProblem.Errors.TryGetValue($"{nameof(Example.Content)}", out var errors);

        errors.ShouldNotBeNull();
        errors.Length.ShouldBe(1);
        errors.ShouldContain($"'{nameof(Example.Content)}' must not be empty.");
    }

    [Test]
    public async Task ShouldReturnNotFoundWhenExampleDoesNotExist()
    {
        var command = new UpdateExampleCommand(Guid.Empty, "test");

        using var client = CreateHttpClient();

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

        await using var context = CreateDbContext();
        await context.AddAsync(example1);
        await context.AddAsync(example2);
        await context.SaveChangesAsync();

        var command = new UpdateExampleCommand(example2.Id, example1.Content);

        using var client = CreateHttpClient();

        var response = await client.PutAsJsonAsync(Endpoint, command);
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var errors = await response.GetResultErrorsAsync();
        errors.ShouldNotBeNull();
        errors.Length.ShouldBe(1);
        errors.ShouldContain($"{nameof(Example)} with '{command.Content}' content already exists.");
    }

    [Test]
    public async Task ShouldReturnNoContentWhenExampleContentHasBeenUpdated()
    {
        var example = new Example { Content = "test" };

        await using var context = CreateDbContext();
        await context.AddAsync(example);
        await context.SaveChangesAsync();

        var command = new UpdateExampleCommand(example.Id, "new-content");

        using var client = CreateHttpClient();

        var response = await client.PutAsJsonAsync(Endpoint, command);
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }
}
