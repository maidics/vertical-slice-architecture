using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace VsaTemplate.Tests.TestInfrastructure.WebTests;

public sealed class EndpointRouteBuilderSpy : IEndpointRouteBuilder
{
    public EndpointRouteBuilderSpy(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        DataSources = new List<EndpointDataSource>();
    }

    public IApplicationBuilder CreateApplicationBuilder() => null!;

    public IServiceProvider ServiceProvider { get; }
    public ICollection<EndpointDataSource> DataSources { get; }

    public List<Endpoint> GetEndpoints() => DataSources.SelectMany(x => x.Endpoints).ToList();
}
