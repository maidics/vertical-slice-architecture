using Aspire.Hosting;
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

        ServiceScope.Dispose();
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

        return _app.CreateHttpClient(Services.WebApi, "http");
    }
}
