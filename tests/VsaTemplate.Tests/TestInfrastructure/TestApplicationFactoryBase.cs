using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace VsaTemplate.Tests.TestInfrastructure;

public abstract class TestApplicationFactoryBase(string connectionString)
    : WebApplicationFactory<Program>
{
    protected const string EnvironmentName = "Testing";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(EnvironmentName);
        builder.UseSetting($"ConnectionStrings:{Shared.Services.Database}", connectionString);
    }
}
