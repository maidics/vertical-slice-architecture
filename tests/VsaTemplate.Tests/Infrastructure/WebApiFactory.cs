using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using VsaTemplate.Common.Interfaces;
using VsaTemplate.Infrastructure;
using VsaTemplate.Tests.Infrastructure.TemplateTests;

namespace VsaTemplate.Tests.Infrastructure;

public sealed class WebApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("ConnectionStrings:VsaTemplateDb", connectionString);

        builder.ConfigureServices(services =>
        {
            services
                .RemoveAll<IUser>()
                .AddTransient(_ =>
                {
                    var mock = new Mock<IUser>();

                    mock.SetupGet(x => x.Roles).Returns(Testing.GetUserRoles);
                    mock.SetupGet(x => x.Id).Returns(Testing.GetUserId);

                    return mock.Object;
                });

            services
                .RemoveAll<IDomainEventDispatcher>()
                .AddScoped(serviceProvider =>
                {
                    var dispatcher = new DomainEventDispatcher(serviceProvider);

                    return new DomainEventDispatcherSpy(dispatcher);
                })
                .AddScoped<IDomainEventDispatcher>(serviceProvider =>
                    serviceProvider.GetRequiredService<DomainEventDispatcherSpy>()
                );

#if (AddTemplateTests)
            services.AddDbContext<TemplateTestDbContext>(
                (sp, options) =>
                {
                    options
                        .UseInMemoryDatabase("TemplateTestDb")
                        .AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                }
            );
#endif
        });
    }
}
