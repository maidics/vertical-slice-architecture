using VsaTemplate.Common.Interfaces.Features;
using VsaTemplate.Infrastructure.Database;

namespace VsaTemplate.Tests.TestInfrastructure.WebTests;

//TODO: move E2E members to another base class for performance - move this to unit testing & rename for clarity
public abstract class EndpointTestBase<TEndpoint> : IEndpointTests
    where TEndpoint : IEndpoint
{
    [ClassDataSource<WebTestFixture>(Shared = SharedType.PerTestSession)]
    public required WebTestFixture Fixture { get; init; }

    public HttpClient Client = null!;
    public ApplicationDbContext DbContext = null!;

    protected static string Prefix => TEndpoint.Prefix;
    protected static string[] Tags => TEndpoint.Tags;

    public abstract void ShouldHaveCorrectPrefix();
    public abstract void ShouldHaveCorrectTags();

    [Before(Test)]
    public async Task ResetAsync()
    {
        await Fixture.ResetAsync();
        Client?.Dispose();

        if (DbContext is not null)
            await DbContext.DisposeAsync();

        Client = Fixture.CreateHttpClient();
        DbContext = Fixture.CreateDbContext();
    }
}
