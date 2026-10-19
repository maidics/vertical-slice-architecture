using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.Engine;
using ModularPipelines.Http;
using ModularPipelines.Modules;
using ModularPipelines.Options;
using VsaTemplate.Pipeline.Settings;

namespace VsaTemplate.Pipeline.Modules;

[DependsOn<TestVariantsModule>]
public sealed class GitHubOidcModule : Module<string>
{
    private readonly GitHubSettings _settings;
    private readonly ISecretRegistry _secretRegistry;

    public GitHubOidcModule(IOptions<GitHubSettings> options, ISecretRegistry secretRegistry)
    {
        _secretRegistry = secretRegistry;
        _settings = options.Value;
    }

    protected override async Task<string?> ExecuteAsync(
        IModuleContext context,
        CancellationToken cancellationToken
    )
    {
        using var response = await GetTokenAsync(
            _settings,
            context.Network.Http,
            cancellationToken
        );

        return await EnsureTokenExistAsync(response, _secretRegistry, cancellationToken);
    }

    public static async Task<HttpResponseMessage> GetTokenAsync(
        GitHubSettings settings,
        IHttp http,
        CancellationToken cancellationToken
    )
    {
        var target = new Uri(
            $"{settings.OidcRequestUrl}&audience={Uri.EscapeDataString("https://nuget.org")}"
        );

        using var request = new HttpRequestMessage(HttpMethod.Get, target);
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            settings.OidcRequestToken // this is masked by SecretValue
        );

        var options = new HttpOptions(request)
        {
            ThrowOnNonSuccessStatusCode = true, // no token in response; safe to throw
            LogSettings = HttpLoggingOptions.None with
            {
                LogResponse = false, // successful responses are not logged
                LogResponseBody = false,
                LogStatusCode = true,
                SensitiveHeaderNames = ["Authorization"], // SecretValue masked additionally
            },
        };

        return await http.SendAsync(options, cancellationToken);
    }

    public static async Task<string> EnsureTokenExistAsync(
        HttpResponseMessage response,
        ISecretRegistry secretRegistry,
        CancellationToken cancellationToken
    )
    {
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
        var token = json.GetProperty("token").GetString();

        if (string.IsNullOrEmpty(token))
            throw new InvalidOperationException(
                "No OIDC token found in response under: \"token\" key."
            );

        secretRegistry.AddSecret(token);

        return token;
    }
}
