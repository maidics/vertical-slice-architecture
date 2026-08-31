using System.Net;
using System.Net.Http.Json;
using VsaTemplate.Features.Examples;
using VsaTemplate.Features.Examples.Create;
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
    public async Task ShouldReturnBadRequestIfContentIsEmpty()
    {
        var command = new CreateExampleCommand(string.Empty);

        using var client = CreateHttpClient();

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

        await using var context = CreateDbContext();

        await context.AddAsync(example);
        await context.SaveChangesAsync();

        var command = new CreateExampleCommand(example.Content);

        using var client = CreateHttpClient();

        var response = await client.PostAsJsonAsync(Endpoint, command);
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);

        var errors = await response.GetResultErrorsAsync();
        errors.ShouldNotBeNull();
        errors.Length.ShouldBe(1);
        errors.ShouldContain($"{nameof(Example)} already exists with content: {command.Content}");
    }
}
