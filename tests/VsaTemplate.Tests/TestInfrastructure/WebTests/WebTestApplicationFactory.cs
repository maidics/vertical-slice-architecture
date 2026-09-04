using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace VsaTemplate.Tests.TestInfrastructure.WebTests;

public sealed class WebTestApplicationFactory(string connectionString)
    : TestApplicationFactoryBase(connectionString: connectionString)
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ConfigureBase(builder);

        builder.ConfigureServices(services =>
        {
            services.Configure<PasswordHasherOptions>(o => o.IterationCount = 1);
        });
    }
}
