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

    // TODO: add skip logic

    protected override async Task<string?> ExecuteAsync(
        IModuleContext context,
        CancellationToken cancellationToken
    )
    {
        using var response = await GetResponseAsync(
            _settings,
            context.Network.Http,
            cancellationToken
        );

        return await EnsureTokenExistAsync(response, _secretRegistry, cancellationToken);
    }

    public static async Task<HttpResponseMessage> GetResponseAsync(
        GitHubSettings settings,
        IHttp http,
        CancellationToken cancellationToken
    )
    {
        var target = new Uri(
            $"{settings.OidcRequestUrl}&audience={Uri.EscapeDataString("https://www.nuget.org")}"
        );

        using var request = new HttpRequestMessage(HttpMethod.Get, target);
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            settings.OidcRequestToken
        );

        var options = new HttpOptions(request)
        {
            ThrowOnNonSuccessStatusCode = true, // request failed: no token in response, safe to throw
            LoggingType = HttpLoggingType.StatusCode,
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
        var token = json.GetProperty("value").GetString();

        if (string.IsNullOrEmpty(token))
            throw new InvalidOperationException(
                "OIDC token is null or empty in response under: \"value\" key."
            );

        //secretRegistry.AddSecret(token);

        return token;
    }
}
