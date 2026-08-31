using VsaTemplate.Common.Interfaces.Features;
using VsaTemplate.Infrastructure.Database;

namespace VsaTemplate.Tests.TestInfrastructure.WebTests;

public abstract class EndpointTestBase<TEndpoint> : IEndpointTests
    where TEndpoint : IEndpoint
{
    [ClassDataSource<WebTestFixture>(Shared = SharedType.PerTestSession)]
    public required WebTestFixture Fixture { get; init; }

    public HttpClient CreateHttpClient() => Fixture.CreateHttpClient();

    public ApplicationDbContext CreateDbContext() => Fixture.CreateDbContext();

    protected static string Prefix => TEndpoint.Prefix;
    protected static string[] Tags => TEndpoint.Tags;

    protected abstract string Endpoint { get; }

    public abstract void ShouldHaveCorrectPrefix();
    public abstract void ShouldHaveCorrectTags();

    [Before(Test)]
    public async Task ResetAsync()
    {
        await Fixture.ResetAsync();
    }
}
