using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Projects;
using VsaTemplate.FunctionalTests.Infrastructure;
using VsaTemplate.Shared;

namespace VsaTemplate.FunctionalTests;

[SetUpFixture]
public class TestSetUpFixture
{
    public static IServiceScopeFactory ScopeFactory { get; private set; } = null!;
    public static TestDatabase? Database { get; private set; }

    private static WebApiFactory? _factory;
    private static DistributedApplication? _app;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

        var builder =
            await DistributedApplicationTestingBuilder.CreateAsync<VsaTemplate_TestAppHost>(
                args: [],
                configureBuilder: (options, _) =>
                {
                    options.DisableDashboard = true;
                },
                cts.Token
            );

        builder.Configuration["ASPIRE_ALLOW_UNSECURED_TRANSPORT"] = "true";

        _app = await builder.BuildAsync(cts.Token).WaitAsync(cts.Token);

        await _app.StartAsync(cts.Token).WaitAsync(cts.Token);

        await _app.ResourceNotifications.WaitForResourceHealthyAsync(Services.Database, cts.Token);

        var connectionString = await _app.GetConnectionStringAsync(Services.Database, cts.Token);
        ArgumentNullException.ThrowIfNull(connectionString);

        _factory = new WebApiFactory(connectionString);
        ScopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
        Database = await TestDatabase.CreateAsync(connectionString);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (Database is not null)
            await Database.DisposeAsync();
        if (_app is not null)
            await _app.DisposeAsync();
        if (_factory is not null)
            await _factory.DisposeAsync();
    }

    public static HttpClient CreateHttpClient()
    {
        ArgumentNullException.ThrowIfNull(_factory);

        return _factory.CreateClient();
    }
}
