using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VsaTemplate.Common.Interfaces;

namespace VsaTemplate.Tests.TestInfrastructure.FunctionalTests;

public class FunctionalTestWebApplicationFactory(string connectionString)
    : TestApplicationFactoryBase(connectionString: connectionString)
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureServices(services =>
        {
            services
                .RemoveAll<IUser>()
                .AddScoped<FunctionalTestUser>()
                .AddScoped<IUser>(sp => sp.GetRequiredService<FunctionalTestUser>());
        });
    }
}
