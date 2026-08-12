using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace VsaTemplate.FunctionalTests.Infrastructure.TemplateTests;

// this is reset per test - used via TemplateTestBase
public sealed class TemplateTesting : IEndpointRouteBuilder
{
    public TemplateTesting()
    {
        Services = new ServiceCollection();
        //Services.AddRouting(); - is this required to call?
        DataSources = new List<EndpointDataSource>();
    }

    public IApplicationBuilder CreateApplicationBuilder() =>
        new ApplicationBuilder(ServiceProvider);

    public IServiceCollection Services { get; }
    public IServiceProvider ServiceProvider => Services.BuildServiceProvider();

    public ICollection<EndpointDataSource> DataSources { get; }

    public List<Endpoint> GetEndpoints() => DataSources.SelectMany(x => x.Endpoints).ToList();
}
