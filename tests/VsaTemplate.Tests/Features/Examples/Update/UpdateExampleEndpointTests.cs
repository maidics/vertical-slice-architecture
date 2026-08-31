using System.Net;
using System.Net.Http.Json;
using VsaTemplate.Features.Examples;
using VsaTemplate.Features.Examples.Update;
using VsaTemplate.Tests.TestInfrastructure;
using VsaTemplate.Tests.TestInfrastructure.WebTests;

namespace VsaTemplate.Tests.Features.Examples.Update;

public sealed class UpdateExampleEndpointTests : EndpointTestBase<UpdateExampleEndpoint>
{
    protected override string Endpoint => "api/example";

    [Test]
    public override void ShouldHaveCorrectPrefix()
    {
        Prefix.ShouldBe(nameof(Example).ToLower());
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
}
