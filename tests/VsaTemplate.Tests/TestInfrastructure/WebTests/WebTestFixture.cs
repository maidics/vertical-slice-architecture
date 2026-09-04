using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Projects;
using TUnit.Core.Interfaces;
using VsaTemplate.Infrastructure.Database;
using VsaTemplate.Shared;

namespace VsaTemplate.Tests.TestInfrastructure.WebTests;

public sealed class WebTestFixture : IAsyncInitializer, IAsyncDisposable
{
    private DistributedApplication? _app;
    private WebTestApplicationFactory? _factory;

    public IServiceScopeFactory ScopeFactory { get; private set; } = null!;

    private TestDatabase? _database;

    public async Task InitializeAsync()
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

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

        var connectionString = await _app.GetConnectionStringAsync(Services.Database, cts.Token);
        ArgumentNullException.ThrowIfNull(connectionString);

        _factory = new WebTestApplicationFactory(connectionString);
        _factory.UseKestrel(0);
        _factory.StartServer();
        ScopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();

        using var scope = ScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync(CancellationToken.None);

        _database = await TestDatabase.CreateAsync(connectionString);
        await _database.SeedRolesAsync(scope.ServiceProvider);
    }

    public async ValueTask DisposeAsync()
    {
        if (_factory is not null)
            await _factory.DisposeAsync();
        if (_database is not null)
            await _database.DisposeAsync();
        if (_app is not null)
            await _app.DisposeAsync();
    }

    public async Task ResetAsync()
    {
        ArgumentNullException.ThrowIfNull(_database);

        await _database.ResetAsync();
    }

    public HttpClient CreateHttpClient()
    {
        ArgumentNullException.ThrowIfNull(_factory);

        return _factory.CreateClient();
    }
}
