using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Projects;
using TUnit.Core.Interfaces;
using VsaTemplate.Infrastructure.Database;
using VsaTemplate.Shared;

namespace VsaTemplate.Tests.TestInfrastructure.FunctionalTests;

public sealed class FunctionalTestFixture : IAsyncInitializer, IAsyncDisposable
{
    private DistributedApplication? _app;
    private FunctionalTestWebApplicationFactory? _factory;

    private IServiceScopeFactory _scopeFactory = null!;

    private TestDatabase? _database;
    public IServiceScope ServiceScope { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

        var builder =
            await DistributedApplicationTestingBuilder.CreateAsync<VsaTemplate_TestAppHost>(
                args: [],
                configureBuilder: (options, settings) =>
                {
                    options.DisableDashboard = true;
                    settings.EnvironmentName = TestingEnvironments.Functional;
                },
                cts.Token
            );

        builder.Configuration["ASPIRE_ALLOW_UNSECURED_TRANSPORT"] = "true";

        _app = await builder.BuildAsync(cts.Token).WaitAsync(cts.Token);

        await _app.StartAsync(cts.Token).WaitAsync(cts.Token);

        await _app.ResourceNotifications.WaitForResourceHealthyAsync(Services.Database, cts.Token);

        var connectionString = await _app.GetConnectionStringAsync(Services.Database, cts.Token);
        ArgumentNullException.ThrowIfNull(connectionString);

        _factory = new FunctionalTestWebApplicationFactory(connectionString);
        _scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();

        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync(CancellationToken.None);

        _database = await TestDatabase.CreateAsync(connectionString);
    }

    public async ValueTask DisposeAsync()
    {
        if (_factory is not null)
            await _factory.DisposeAsync();
        if (_database is not null)
            await _database.DisposeAsync();
        if (_app is not null)
            await _app.DisposeAsync();

        ServiceScope?.Dispose();
    }

    public async Task ResetAsync()
    {
        ArgumentNullException.ThrowIfNull(_database);

        await _database.ResetAsync();

        ServiceScope?.Dispose();
        ServiceScope = _scopeFactory.CreateScope();
    }
}
