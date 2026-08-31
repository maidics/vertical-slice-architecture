using Aspire.Hosting;
using Aspire.Hosting.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Projects;
using TUnit.Core.Interfaces;
using VsaTemplate.Infrastructure.Database;
using VsaTemplate.Shared;

namespace VsaTemplate.Tests.TestInfrastructure.WebTests;

public sealed class WebTestFixture : IAsyncInitializer, IAsyncDisposable
{
    private DistributedApplication? _app;
    private string _connectionString = null!;
    private TestDatabase? _database;

    public async Task InitializeAsync()
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

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

        // awaits /health endpoint which is only set if Environment is Development
        await _app.ResourceNotifications.WaitForResourceHealthyAsync(Services.WebApi, cts.Token);

        var connectionString = await _app.GetConnectionStringAsync(Services.Database, cts.Token);
        ArgumentNullException.ThrowIfNull(connectionString);

        _connectionString = connectionString;
        _database = await TestDatabase.CreateAsync(connectionString);
    }

    public async ValueTask DisposeAsync()
    {
        if (_database is not null)
            await _database.DisposeAsync();
        if (_app is not null)
            await _app.DisposeAsync();
    }

    public async Task ResetAsync()
    {
        if (_database is not null)
            await _database.ResetAsync();
    }

    public HttpClient CreateHttpClient()
    {
        ArgumentNullException.ThrowIfNull(_app);

        var client = _app.CreateHttpClient(Services.WebApi, "http");
        client.Timeout = TimeSpan.FromSeconds(15);

        return client;
    }

    public ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connectionString)
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;

        return new ApplicationDbContext(options);
    }
}
