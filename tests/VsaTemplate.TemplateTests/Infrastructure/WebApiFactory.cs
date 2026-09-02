using FluentValidation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VsaTemplate.Common.Extensions;
using VsaTemplate.Common.Interfaces;
using VsaTemplate.Common.Services;

namespace VsaTemplate.TemplateTests.Infrastructure;

public sealed class WebApiFactory(string connectionString)
    : WebApplicationFactory<VsaTemplate.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("TemplateTesting");

        builder.UseSetting("ConnectionStrings:VsaTemplateDb", connectionString);

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
                .AddRequestHandlers(typeof(WebApiFactory).Assembly)
                .AddDomainEventHandlers(typeof(WebApiFactory).Assembly)
                .AddValidatorsFromAssembly(typeof(WebApiFactory).Assembly);

            services.AddScoped<EndpointRouteBuilderSpy>();
        });
    }
}
