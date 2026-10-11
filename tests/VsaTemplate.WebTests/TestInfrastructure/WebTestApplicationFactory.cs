using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace VsaTemplate.WebTests.TestInfrastructure;

public sealed class WebTestApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public WebTestApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;

        UseKestrel(0); // dynamic port, clients created via CreateClient talk to it over the network
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting($"ConnectionStrings:{Shared.Services.Database}", _connectionString);

        builder.ConfigureServices(services =>
        {
            services.Configure<PasswordHasherOptions>(o => o.IterationCount = 1);
        });
    }
}
