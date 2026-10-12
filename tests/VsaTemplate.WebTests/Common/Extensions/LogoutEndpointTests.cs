using System.Net;

namespace VsaTemplate.WebTests.Common.Extensions;

public sealed class LogoutEndpointTests : EndpointTestBase
{
    protected override string Endpoint => "api/identity/logout";

    [Test]
    public async Task ShouldReturnUnauthorizedWhenAnonymous()
    {
        using var client = CreateHttpClient();

        var response = await client.PostAsync(Endpoint, null);
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task ShouldReturnOkAndSignUserOut()
    {
        using var client = await LogInAsync();

        var response = await client.PostAsync(Endpoint, null);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var afterLogout = await client.PostAsync(Endpoint, null);
        afterLogout.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}
