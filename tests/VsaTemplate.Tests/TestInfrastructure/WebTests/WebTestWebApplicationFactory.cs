using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace VsaTemplate.Tests.TestInfrastructure.WebTests;

public sealed class WebTestWebApplicationFactory(string connectionString)
    : WebApplicationFactory<VsaTemplate.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("ConnectionStrings:VsaTemplateDb", connectionString);
    }
}
