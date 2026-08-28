using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using VsaTemplate.Common.Interfaces;
using VsaTemplate.Common.Services;

namespace VsaTemplate.Tests.TestInfrastructure.FunctionalTests;

public class FunctionalTestWebApplicationFactory(string connectionString)
    : WebApplicationFactory<VsaTemplate.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("ConnectionStrings:VsaTemplateDb", connectionString);

        builder.ConfigureServices(services =>
        {
            services
                .RemoveAll<IUser>()
                .AddScoped<FunctionalTestUser>()
                .AddScoped<IUser>(sp => sp.GetRequiredService<FunctionalTestUser>());

            services
                .RemoveAll<IDomainEventDispatcher>()
                .AddScoped(serviceProvider =>
                {
                    var dispatcher = new DomainEventDispatcher(
                        serviceProvider,
                        serviceProvider.GetRequiredService<ILogger<DomainEventDispatcher>>()
                    );

                    return new DomainEventDispatcherSpy(dispatcher);
                })
                .AddScoped<IDomainEventDispatcher>(serviceProvider =>
                    serviceProvider.GetRequiredService<DomainEventDispatcherSpy>()
                );
        });
    }
}
