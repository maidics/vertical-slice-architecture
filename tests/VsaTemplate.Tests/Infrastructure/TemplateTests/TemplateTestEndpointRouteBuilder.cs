using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace VsaTemplate.Tests.Infrastructure.TemplateTests;

public sealed class TemplateTestEndpointRouteBuilder : IEndpointRouteBuilder
{
    public TemplateTestEndpointRouteBuilder()
    {
        Services = new ServiceCollection();
        //Services.AddRouting(); - is this required to call?

        ServiceProvider = Services.BuildServiceProvider();
        DataSources = new List<EndpointDataSource>();
    }

    public IApplicationBuilder CreateApplicationBuilder() =>
        new ApplicationBuilder(ServiceProvider);

    private IServiceCollection Services { get; }
    public IServiceProvider ServiceProvider { get; }
    public ICollection<EndpointDataSource> DataSources { get; }

    public List<Endpoint> GetEndpoints() => DataSources.SelectMany(x => x.Endpoints).ToList();
}
