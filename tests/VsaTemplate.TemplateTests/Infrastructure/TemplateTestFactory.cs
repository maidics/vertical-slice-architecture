using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VsaTemplate.Common.Extensions;
using VsaTemplate.Common.Interfaces;
using VsaTemplate.Common.Services;
using VsaTemplate.Tests.TestInfrastructure;

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
                .RemoveAll<IDomainEventDispatcher>()
                .AddScoped<DomainEventDispatcher>()
                .AddScoped<DomainEventDispatcherSpy>()
                .AddScoped<IDomainEventDispatcher>(sp =>
                    sp.GetRequiredService<DomainEventDispatcherSpy>()
                );

            services
                .RemoveAll<IRequestHandler>()
                .RemoveAll<IDomainEventHandler<IDomainEvent>>()
                .RemoveAll<IValidator<IRequest>>()
                .AddRequestHandlers(typeof(TemplateTestFactory).Assembly)
                .AddDomainEventHandlers(typeof(TemplateTestFactory).Assembly)
                .AddValidatorsFromAssembly(typeof(TemplateTestFactory).Assembly);

            services.AddScoped<EndpointRouteBuilderSpy>();
        });
    }
}
