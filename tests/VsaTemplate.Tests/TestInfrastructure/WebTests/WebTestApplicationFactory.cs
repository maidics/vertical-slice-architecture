using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace VsaTemplate.Tests.TestInfrastructure.WebTests;

public sealed class WebTestApplicationFactory(string connectionString)
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("ConnectionStrings:VsaTemplateDb", connectionString);

        /*
        builder.ConfigureServices(services =>
        {
            services.Configure<PasswordHasherOptions>(o => o.IterationCount = 1);
        });
        */
    }
}
