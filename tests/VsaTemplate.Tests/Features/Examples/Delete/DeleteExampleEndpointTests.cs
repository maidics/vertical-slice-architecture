using System.Net;
using Microsoft.EntityFrameworkCore;
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
    public async Task ShouldReturnNoContentIfExampleHasBeenDeleted()
    {
        var example = new Example { Content = "test" };

        await using var context = CreateDbContext();

        await context.Examples.AddAsync(example);
        await context.SaveChangesAsync();

        using var client = CreateHttpClient();

        var response = await client.DeleteAsync(Endpoint + $"/{example.Id}");
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var deleted = await context.Examples.FirstOrDefaultAsync(e => e.Id == example.Id);
        deleted.ShouldBeNull();
    }
}
