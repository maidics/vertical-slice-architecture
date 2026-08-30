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

        // The real "webapi" resource (started via the Aspire AppHost in WebTestFixture) is the
        // one actually serving HTTP traffic and already runs DatabaseInitialiser against this
        // same connection string. This factory only exists to hand tests a DI container, so it
        // must not initialise the database again - doing so raced with the webapi process's own
        // migration/seed and corrupted the SQLite file (surfaced as "disk I/O error", which left
        // the webapi process crashed and requests hanging until the client timeout).
        builder.UseSetting("SkipDatabaseInitialisation", "true");
    }
}
