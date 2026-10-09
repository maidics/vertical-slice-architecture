using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VsaTemplate.Common.Extensions;
using VsaTemplate.Common.Interfaces;
using VsaTemplate.Tests.TestInfrastructure.WebTests;

namespace VsaTemplate.Tests.TestInfrastructure.TemplateTests;

public sealed class TemplateTestWebApplicationFactory(string connectionString)
    : TestApplicationFactoryBase(connectionString: connectionString)
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices(services =>
        {
            services
                .RemoveAll<IRequestHandler>()
                .RemoveAll<IValidator<IRequest>>()
                .AddRequestHandlers(typeof(TemplateTestWebApplicationFactory).Assembly)
                .AddValidatorsFromAssembly(typeof(TemplateTestWebApplicationFactory).Assembly);
        });
    }
}
