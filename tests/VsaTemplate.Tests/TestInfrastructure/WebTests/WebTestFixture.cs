using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Testing;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Projects;
using TUnit.Core.Interfaces;
using VsaTemplate.Common.Constants;
using VsaTemplate.Shared;

namespace VsaTemplate.Tests.TestInfrastructure.WebTests;

public sealed class WebTestFixture : IAsyncInitializer, IAsyncDisposable
{
    private DistributedApplication? _app;
    private WebTestWebApplicationFactory? _factory;

    private IServiceScopeFactory _scopeFactory = null!;

    private TestDatabase? _database;
    public IServiceScope ServiceScope { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

        var builder = await DistributedApplicationTestingBuilder.CreateAsync<VsaTemplate_AppHost>(
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

        // Wait for the real "webapi" resource to actually be running before touching the
        // database ourselves - it runs DatabaseInitialiser (create schema + seed) on startup,
        // and racing that from here (e.g. via the factory below or the Respawner connection)
        // corrupts the shared SQLite file instead of just failing fast. The resource reaching
        // the "Running" state is not a strong enough signal on its own: Aspire reports it as
        // soon as the process starts, well before Kestrel has bound (and therefore before
        // DatabaseInitialiser has finished), so we additionally probe over HTTP - any response
        // (even an error one) proves Kestrel is listening, which only happens after
        // DatabaseInitialiser has completed in Program.cs.
        await _app.ResourceNotifications.WaitForResourceAsync(
            Services.WebApi,
            KnownResourceStates.Running,
            cts.Token
        );

        await WaitForWebApiReadyAsync(cts.Token);

        var connectionString = await _app.GetConnectionStringAsync(Services.Database, cts.Token);
        ArgumentNullException.ThrowIfNull(connectionString);

        _factory = new WebTestWebApplicationFactory(connectionString);
        _scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
        _database = await TestDatabase.CreateAsync(connectionString);
    }

    public async ValueTask DisposeAsync()
    {
        if (_database is not null)
            await _database.DisposeAsync();
        if (_app is not null)
            await _app.DisposeAsync();
        if (_factory is not null)
            await _factory.DisposeAsync();

        ServiceScope?.Dispose();
    }

    private async Task WaitForWebApiReadyAsync(CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(_app);

        using var client = _app.CreateHttpClient(Services.WebApi, "http");
        client.Timeout = TimeSpan.FromSeconds(5);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var response = await client.GetAsync("/", cancellationToken);
                return;
            }
            catch (Exception) when (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
            }
        }
    }

    public async Task ResetAsync()
    {
        if (_database is not null)
            await _database.ResetAsync();

        ServiceScope?.Dispose();
        ServiceScope = _scopeFactory.CreateScope();

        var roleManager = ServiceScope.ServiceProvider.GetRequiredService<
            RoleManager<IdentityRole<Guid>>
        >();

        foreach (var role in Roles.All)
        {
            await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }
    }

    public HttpClient CreateHttpClient()
    {
        ArgumentNullException.ThrowIfNull(_app);

        var client = _app.CreateHttpClient(Services.WebApi, "http");

        // Safety net: if the webapi resource ever dies or wedges again, fail the test in a few
        // seconds instead of waiting out HttpClient's 100-second default.
        client.Timeout = TimeSpan.FromSeconds(15);

        return client;
    }
}
