using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ModularPipelines.Http;
using Shouldly;
using VsaTemplate.Pipeline.Constants;
using VsaTemplate.Pipeline.Interfaces;
using VsaTemplate.Pipeline.Modules;
using VsaTemplate.Pipeline.Settings;
using VsaTemplate.Pipeline.Tests.TestInfrastructure.BaseClasses;
using VsaTemplate.Pipeline.Tests.TestInfrastructure.Extensions;

namespace VsaTemplate.Pipeline.Tests.Modules;

[InheritsTests]
public sealed class NuGetLoginModuleTests()
    : ModuleTestBase<NuGetLoginModule>(typeof(GitHubOidcModule))
{
    [Test]
    [Arguments("tag")]
    [Arguments("non-tag")]
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

        var module = new NuGetLoginModule(
            options,
            IOptions<NuGetSettings>.Mock().Object,
            IGithubSecretMasker.Mock().Object
        );

        var config = module.GetConfiguration();
        var decision = await config.SkipCondition.ShouldNotBeNull().Invoke(null!);
        decision.ShouldSkip.ShouldBe(SkipDecisions.TagPush(options.Value).Invoke(null!).ShouldSkip);
    }

    [Test]
    public async Task GetApiKeyAsyncShouldReturnHttpResponseMessage()
    {
        using var r = new HttpResponseMessage();

        var http = IHttp.Mock();
        http.SendAsync(Any(), Any()).Returns(r);

        string user = "test";
        string oidcToken = Guid.NewGuid().ToString();

        var settings = new NuGetSettings { User = user };

        var expectedBody = JsonSerializer.Serialize(
            new { username = user, tokenType = NuGetSettings.ApiKeyTokenType }
        );

        using var response = await NuGetLoginModule.GetApiKeyAsync(
            settings,
            http.Object,
            oidcToken,
            CancellationToken.None
        );

        http.SendAsync(
                o =>
                    o.ThrowOnNonSuccessStatusCode
                    && o.LoggingType == HttpLoggingType.StatusCode
                    && o.HttpRequestMessage.Method == HttpMethod.Post
                    && o.HttpRequestMessage.RequestUri is not null
                    && o.HttpRequestMessage.RequestUri.Equals(NuGetSettings.TokenEndpointUri)
                    && o.HttpRequestMessage.Headers.Authorization is not null
                    && o.HttpRequestMessage.Headers.Authorization.Scheme == "Bearer"
                    && o.HttpRequestMessage.Headers.Authorization.Parameter == oidcToken
                    && o.HttpRequestMessage.Content is JsonContent content
                    && JsonSerializer.Serialize(content.Value) == expectedBody,
                Any()
            )
            .WasCalled(Times.Once);
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    public async Task EnsureApiKeyExistAsyncShouldThrowIfApiKeyIsNullOrEmpty(string? apiKey)
    {
        var body = new { apiKey };

        var serialized = JsonSerializer.Serialize(body);

        using var response = new HttpResponseMessage();
        response.Content = new StringContent(serialized);

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            NuGetLoginModule.EnsureApiKeyExistAsync(
                response,
                IGithubSecretMasker.Mock().Object,
                CancellationToken.None
            )
        );

        ex.Message.ShouldBe(
            "Short-lived API key is null or empty in response under: \"apiKey\" key."
        );
    }

    [Test]
    public async Task EnsureApiKeyExistAsyncShouldReturnApiKeyIfExistsAndMaskApiKey()
    {
        string apiKey = Guid.NewGuid().ToString();
        var body = new { apiKey };

        var serialized = JsonSerializer.Serialize(body);

        using var response = new HttpResponseMessage();
        response.Content = new StringContent(serialized);

        var secretMasker = IGithubSecretMasker.Mock();

        var key = await NuGetLoginModule.EnsureApiKeyExistAsync(
            response,
            secretMasker.Object,
            CancellationToken.None
        );
        key.ShouldBe(apiKey);

        secretMasker.Mask(apiKey).WasCalled(Times.Once);
    }
}
