using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VsaTemplate.Common.Interfaces;

namespace VsaTemplate.UnitTests.TestInfrastructure;

public sealed class UnitTestApplicationFactory(string connectionString)
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting($"ConnectionStrings:{Shared.Services.Database}", connectionString);

        builder.ConfigureServices(services =>
        {
            services
                .RemoveAll<IUser>()
                .AddScoped<TestUser>()
                .AddScoped<IUser>(sp => sp.GetRequiredService<TestUser>());
        });
    }
}
