using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using TUnit.Core.Interfaces;

namespace VsaTemplate.Tests.TestInfrastructure.WebTests;

public sealed class EndpointRouteBuilderSpy
    : IEndpointRouteBuilder,
        IAsyncInitializer,
        IAsyncDisposable
{
    private IServiceCollection _services = null!;
    private ServiceProvider _serviceProvider = null!;

    public IApplicationBuilder CreateApplicationBuilder() => throw new NotSupportedException();

    public IServiceProvider ServiceProvider
    {
        get { return _serviceProvider; }
    }

    public ICollection<EndpointDataSource> DataSources { get; private set; } = null!;

    public List<Endpoint> GetEndpoints() => DataSources.SelectMany(x => x.Endpoints).ToList();

    public Task InitializeAsync()
    {
        _services = new ServiceCollection();
        _serviceProvider = _services.BuildServiceProvider();
        DataSources = [];

        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await _serviceProvider.DisposeAsync();
    }
}
