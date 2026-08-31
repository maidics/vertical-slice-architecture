using System.Net;
using VsaTemplate.Features.Examples;
using VsaTemplate.Features.Examples.Delete;
using VsaTemplate.Tests.TestInfrastructure;
using VsaTemplate.Tests.TestInfrastructure.WebTests;

namespace VsaTemplate.Tests.Features.Examples.Delete;

public sealed class DeleteExampleEndpointTests : EndpointTestBase<DeleteExampleEndpoint>
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
    public async Task ShouldReturnNotFoundIfExampleNotExists()
    {
        using var client = CreateHttpClient();

        var response = await client.DeleteAsync(Endpoint + $"/{Guid.Empty}");
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var errors = await response.GetResultErrorsAsync();
        errors.ShouldNotBeNull();
        errors.Length.ShouldBe(1);
        errors.ShouldContain($"{nameof(Example)} not found.");
    }

    [Test]
    public async Task ShouldReturnOkIfExampleHasBeenDeleted()
    {
        var example = new Example { Content = "test" };

        await using var context = CreateDbContext();

        await context.Examples.AddAsync(example);
        await context.SaveChangesAsync();

        using var client = CreateHttpClient();

        var response = await client.DeleteAsync(Endpoint + $"/{example.Id}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }
}
