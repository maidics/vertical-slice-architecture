namespace VsaTemplate.Tests.TestInfrastructure.WebTests;

public sealed class WebTestFixture : TestFixtureBase<WebTestApplicationFactory>
{
    protected override WebTestApplicationFactory CreateFactory(string connectionString)
    {
        return new WebTestApplicationFactory(connectionString);
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        ArgumentNullException.ThrowIfNull(_database);

        using var scope = ScopeFactory.CreateScope();

        await _database.SeedRolesAsync(scope.ServiceProvider);
    }

    public HttpClient CreateHttpClient()
    {
        ArgumentNullException.ThrowIfNull(_factory);

        return _factory.CreateClient();
    }
}
