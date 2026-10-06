using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VsaTemplate.Common.Extensions;
using VsaTemplate.Common.Interfaces;
using VsaTemplate.Tests.TestInfrastructure;
using VsaTemplate.Tests.TestInfrastructure.WebTests;

namespace VsaTemplate.TemplateTests.Infrastructure;

public sealed class TemplateTestFactory(string connectionString)
    : TestApplicationFactoryBase(connectionString: connectionString)
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices(services =>
        {
            services
                .RemoveAll<IUser>()
                .AddScoped<TestUser>()
                .AddScoped<IUser>(sp => sp.GetRequiredService<TestUser>());

            services.AddDbContext<TestDbContext>(
                (sp, options) =>
                {
                    options
                        .UseInMemoryDatabase("TestDb")
                        .AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                }
            );

            services
                .RemoveAll<IRequestHandler>()
                .RemoveAll<IValidator<IRequest>>()
                .AddRequestHandlers(typeof(TemplateTestFactory).Assembly)
                .AddValidatorsFromAssembly(typeof(TemplateTestFactory).Assembly);

            services.AddScoped<EndpointRouteBuilderSpy>();
        });
    }
}
