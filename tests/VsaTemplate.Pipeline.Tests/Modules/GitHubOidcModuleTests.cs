using System.Text.Json;
using Microsoft.Extensions.Options;
using ModularPipelines.Http;
using Shouldly;
using VsaTemplate.Pipeline.Interfaces;
using VsaTemplate.Pipeline.Modules;
using VsaTemplate.Pipeline.Settings;
using VsaTemplate.Pipeline.Tests.TestInfrastructure.BaseClasses;
using VsaTemplate.Pipeline.Tests.TestInfrastructure.Extensions;

namespace VsaTemplate.Pipeline.Tests.Modules;

[InheritsTests]
public sealed class GitHubOidcModuleTests()
    : ModuleTestBase<GitHubOidcModule>(typeof(TestVariantsModule))
{
    [Test]
    [Arguments("tag")]
    [Arguments("not-tag")]
    [Skip("Temporarily disabled")]
    public async Task ShouldSkipOnNonTagPushes(string refType)
    {
        var options = Options.Create(
            new GitHubSettings
            {
                RefType = refType,
                RefName = "test",
                EventName = "test",
                OidcRequestUrl = "test",
                OidcRequestToken = "test",
            }
        );

        var secretMasker = IGithubSecretMasker.Mock();

        var module = new GitHubOidcModule(options, secretMasker.Object);

        var config = module.GetConfiguration();
        await config.ShouldSkipOnNonTagPushAsync(options.Value.IsTagPush);
    }

    [Test]
    public async Task GetResponseAsyncShouldReturnHttpResponseMessage()
    {
        using var r = new HttpResponseMessage();

        var http = IHttp.Mock();
        http.SendAsync(Any(), Any()).Returns(r);

        string oidcRequestUrl = "https://www.request-url.com/";
        string oidcRequestToken = Guid.NewGuid().ToString();

        var settings = new GitHubSettings
        {
            RefType = "test",
            RefName = "test",
            EventName = "test",
            OidcRequestUrl = oidcRequestUrl,
            OidcRequestToken = oidcRequestToken,
        };

        using var response = await GitHubOidcModule.GetResponseAsync(
            settings,
            http.Object,
            CancellationToken.None
        );

        http.SendAsync(
                o =>
                    o.ThrowOnNonSuccessStatusCode
                    && o.LoggingType == HttpLoggingType.StatusCode
                    && o.HttpRequestMessage.Method == HttpMethod.Get
                    && o.HttpRequestMessage.RequestUri is not null
                    && o.HttpRequestMessage.RequestUri.Equals(settings.GetOidcRequestUri()),
                Any()
            )
            .WasCalled(Times.Once);
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    public async Task EnsureTokenExistAsyncShouldThrowIfJwtIsNullOrEmpty(string? jwt)
    {
        var body = new { value = jwt };

        var serialized = JsonSerializer.Serialize(body);

        using var response = new HttpResponseMessage();
        response.Content = new StringContent(serialized);

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            GitHubOidcModule.EnsureTokenExistAsync(
                response,
                IGithubSecretMasker.Mock().Object,
                CancellationToken.None
            )
        );

        ex.Message.ShouldBe("OIDC token is null or empty in response under: \"value\" key.");
    }

    [Test]
    public async Task EnsureTokenExistAsyncShouldReturnTokenIfExistsAndMaskToken()
    {
        string jwt = Guid.NewGuid().ToString();
        var body = new { value = jwt };

        var serialized = JsonSerializer.Serialize(body);

        using var response = new HttpResponseMessage();
        response.Content = new StringContent(serialized);

        var secretMasker = IGithubSecretMasker.Mock();

        var token = await GitHubOidcModule.EnsureTokenExistAsync(
            response,
            secretMasker,
            CancellationToken.None
        );
        token.ShouldBe(jwt);

        secretMasker.Mask(jwt).WasCalled(Times.Once);
    }
}
