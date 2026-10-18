using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VsaTemplate.Pipeline.Settings;

namespace VsaTemplate.Pipeline.Extensions;

public static class SettingsRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddGitHubSettings(IConfiguration configuration)
        {
            services
                .AddOptions<GitHubSettings>()
                .Configure(settings =>
                {
                    settings.RefType = configuration["GITHUB_REF_TYPE"] ?? string.Empty;
                    settings.RefName = configuration["GITHUB_REF_NAME"] ?? string.Empty;
                    settings.EventName = configuration["GITHUB_EVENT_NAME"] ?? string.Empty;
                    settings.OidcRequestUrl =
                        configuration["ACTIONS_ID_TOKEN_REQUEST_URL"] ?? string.Empty;
                    settings.OidcRequestToken =
                        configuration["ACTIONS_ID_TOKEN_REQUEST_TOKEN"] ?? string.Empty;
                })
                .ValidateDataAnnotations()
                .Validate(
                    settings =>
                        !settings.IsTagPush || !string.IsNullOrWhiteSpace(settings.OidcRequestUrl),
                    "ACTIONS_ID_TOKEN_REQUEST_URL is required on tag pushes."
                )
                .Validate(
                    settings =>
                        !settings.IsTagPush
                        || !string.IsNullOrWhiteSpace(settings.OidcRequestToken),
                    "ACTIONS_ID_TOKEN_REQUEST_TOKEN is required on tag pushes."
                )
                .ValidateOnStart();

            return services;
        }

        public IServiceCollection AddNuGetSettings(IConfiguration configuration)
        {
            services
                .AddOptions<NuGetSettings>()
                .Configure(settings => settings.User = configuration["NUGET_USER"] ?? string.Empty)
                .ValidateDataAnnotations(); // this will throw only when accessing Options<NuGetSettings>.Value (value only required for tag pushes)

            return services;
        }
    }
}
