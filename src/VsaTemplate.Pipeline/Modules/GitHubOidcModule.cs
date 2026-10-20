using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ModularPipelines.Attributes;
using ModularPipelines.Configuration;
using ModularPipelines.Context;
using ModularPipelines.Http;
using ModularPipelines.Modules;
using ModularPipelines.Options;
using VsaTemplate.Pipeline.Constants;
using VsaTemplate.Pipeline.Interfaces;
using VsaTemplate.Pipeline.Settings;

namespace VsaTemplate.Pipeline.Modules;

[DependsOn<TestVariantsModule>]
public sealed class GitHubOidcModule : Module<string>
{
    private readonly GitHubSettings _settings;
    private readonly IGithubSecretMasker _secretMasker;

    public GitHubOidcModule(IOptions<GitHubSettings> options, IGithubSecretMasker secretMasker)
    {
        _secretMasker = secretMasker;
        _settings = options.Value;
    }

    protected override ModuleConfiguration Configure()
    {
        return ModuleConfiguration.Create().WithSkipWhen(SkipDecisions.TagPush(_settings)).Build();
    }

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

        return await EnsureTokenExistAsync(response, _secretMasker, cancellationToken);
    }

    public static async Task<HttpResponseMessage> GetResponseAsync(
        GitHubSettings settings,
        IHttp http,
        CancellationToken cancellationToken
    )
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, settings.GetOidcRequestUri());
        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            settings.OidcRequestToken
        );

        var options = new HttpOptions(request)
        {
            ThrowOnNonSuccessStatusCode = true, // request failed: no token in response, safe to throw
            LoggingType = HttpLoggingType.StatusCode, // LogSettings only works with custom HttpClient => potential JWT leak
        };

        return await http.SendAsync(options, cancellationToken);
    }

    public static async Task<string> EnsureTokenExistAsync(
        HttpResponseMessage response,
        IGithubSecretMasker secretMasker,
        CancellationToken cancellationToken
    )
    {
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
        var token = json.GetProperty("value").GetString();

        if (string.IsNullOrEmpty(token))
            throw new InvalidOperationException(
                "OIDC token is null or empty in response under: \"value\" key."
            );

        secretMasker.Mask(token);

        return token;
    }
}
