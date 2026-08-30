using VsaTemplate.Common.Interfaces.Features;

namespace VsaTemplate.Tests.TestInfrastructure.WebTests;

//TODO: add E2E testing members - which confirms the method and its existence
public abstract class EndpointTestBase<TEndpoint> : IEndpointTests
    where TEndpoint : IEndpoint
{
    [ClassDataSource<WebTestFixture>(Shared = SharedType.PerTestSession)]
    public required WebTestFixture Fixture { get; init; }

    public HttpClient Client = null!;

    protected static string Prefix => TEndpoint.Prefix;
    protected static string[] Tags => TEndpoint.Tags;

    public abstract void ShouldHaveCorrectPrefix();
    public abstract void ShouldHaveCorrectTags();

    [Before(Test)]
    public async Task ResetAsync()
    {
        await Fixture.ResetAsync();
        Client?.Dispose();

        Client = Fixture.CreateHttpClient();
    }
}
