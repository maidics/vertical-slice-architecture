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
using VsaTemplate.Pipeline.Extensions;
using VsaTemplate.Pipeline.Interfaces;
using VsaTemplate.Pipeline.Settings;

namespace VsaTemplate.Pipeline.Modules;

[DependsOn<GitHubOidcModule>]
public sealed class NuGetLoginModule : Module<string>
{
    private readonly GitHubSettings _gitHubSettings;
    private readonly NuGetSettings _nuGetSettings;
    private readonly IGithubSecretMasker _secretMasker;

    public NuGetLoginModule(
        IOptions<GitHubSettings> githubOptions,
        IOptions<NuGetSettings> nugetOptions,
        IGithubSecretMasker secretMasker
    )
    {
        _secretMasker = secretMasker;
        _gitHubSettings = githubOptions.Value;
        _nuGetSettings = nugetOptions.Value;
    }

    // protected override ModuleConfiguration Configure()
    // {
    //     return ModuleConfiguration.Create().WithNonTagPushSkip(_gitHubSettings).Build();
    // }

    protected override async Task<string?> ExecuteAsync(
        IModuleContext context,
        CancellationToken cancellationToken
    )
    {
        var result = await context.GetModule<GitHubOidcModule>();
        ArgumentNullException.ThrowIfNull(result.ValueOrDefault);

        using var response = await GetApiKeyAsync(
            _nuGetSettings,
            context.Network.Http,
            result.ValueOrDefault,
            cancellationToken
        );

        return await EnsureApiKeyExistAsync(response, _secretMasker, cancellationToken);
    }

    public static async Task<HttpResponseMessage> GetApiKeyAsync(
        NuGetSettings settings,
        IHttp http,
        string oidcToken,
        CancellationToken cancellationToken
    )
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, NuGetSettings.TokenEndpointUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", oidcToken);
        request.Headers.UserAgent.Add(
            new ProductInfoHeaderValue(nameof(VsaTemplate) + "." + nameof(Pipeline), "1.0.0")
        );
        request.Content = JsonContent.Create(
            new { username = settings.User, tokenType = NuGetSettings.ApiKeyTokenType }
        );

        var options = new HttpOptions(request)
        {
            ThrowOnNonSuccessStatusCode = true,
            LoggingType = HttpLoggingType.StatusCode,
        };

        return await http.SendAsync(options, cancellationToken);
    }

    public static async Task<string> EnsureApiKeyExistAsync(
        HttpResponseMessage response,
        IGithubSecretMasker secretMasker,
        CancellationToken cancellationToken
    )
    {
        var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
        var key = json.GetProperty("apiKey").GetString();

        if (string.IsNullOrEmpty(key))
            throw new InvalidOperationException(
                "Short-lived API key is null or empty in response under: \"apiKey\" key."
            );

        secretMasker.Mask(key);

        return key;
    }
}
