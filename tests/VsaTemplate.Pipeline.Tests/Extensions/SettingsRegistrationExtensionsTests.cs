using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using VsaTemplate.Pipeline.Extensions;
using VsaTemplate.Pipeline.Settings;

namespace VsaTemplate.Pipeline.Tests.Extensions;

public sealed class SettingsRegistrationExtensionsTests
{
    [Test]
    [MethodDataSource(nameof(AddGitHubSettingsShouldThrowOnStartIfAnyConfigurationIsEmptySource))] // tag push always false
    public async Task AddGitHubSettingsShouldThrowOnStartIfAnyConfigurationIsEmpty( // so will not validate OIDC properties
        Dictionary<string, string?> configValues
    )
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(configValues).Build();

        var services = new ServiceCollection();
        services.AddGitHubSettings(config);

        await using var provider = services.BuildServiceProvider();

        Should.Throw<OptionsValidationException>(() =>
            provider.GetRequiredService<IStartupValidator>().Validate()
        );
    }

    public static IEnumerable<
        Func<Dictionary<string, string?>>
    > AddGitHubSettingsShouldThrowOnStartIfAnyConfigurationIsEmptySource()
    {
        yield return () =>
            new Dictionary<string, string?>
            {
                ["GITHUB_REF_TYPE"] = string.Empty,
                ["GITHUB_REF_NAME"] = "test",
                ["GITHUB_EVENT_NAME"] = "test",
            };
        yield return () =>
            new Dictionary<string, string?>
            {
                ["GITHUB_REF_TYPE"] = "test",
                ["GITHUB_REF_NAME"] = null,
                ["GITHUB_EVENT_NAME"] = "test",
            };
        yield return () =>
            new Dictionary<string, string?>
            {
                ["GITHUB_REF_TYPE"] = "test",
                ["GITHUB_REF_NAME"] = "test",
                ["GITHUB_EVENT_NAME"] = null,
            };
    }

    [Test]
    [Arguments("", "token")]
    [Arguments(null, "token")]
    [Arguments("url", "")]
    [Arguments("url", null)]
    public async Task AddGitHubSettingsShouldThrowOnTagPushIfAnyOidcPropertyIsMissing(
        string? oidcRequestUrl,
        string? oidcRequestToken
    )
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["GITHUB_REF_TYPE"] = "tag",
                    ["GITHUB_REF_NAME"] = "test",
                    ["GITHUB_EVENT_NAME"] = "test",
                    ["ACTIONS_ID_TOKEN_REQUEST_URL"] = oidcRequestUrl,
                    ["ACTIONS_ID_TOKEN_REQUEST_TOKEN"] = oidcRequestToken,
                }
            )
            .Build();

        var services = new ServiceCollection();
        services.AddGitHubSettings(config);

        await using var provider = services.BuildServiceProvider();

        var ex = Should.Throw<OptionsValidationException>(() =>
            provider.GetRequiredService<IStartupValidator>().Validate()
        );

        if (string.IsNullOrEmpty(oidcRequestUrl))
            ex.Message.ShouldBe("ACTIONS_ID_TOKEN_REQUEST_URL is required on tag pushes.");
        else
            ex.Message.ShouldBe("ACTIONS_ID_TOKEN_REQUEST_TOKEN is required on tag pushes.");
    }

    [Test]
    [Arguments("", "token")]
    [Arguments("", "")]
    [Arguments("url", "")]
    public async Task AddGitHubSettingsShouldNotThrowIfAnyOidcPropertyIsMissingOnNonTagPush(
        string? oidcRequestUrl,
        string? oidcRequestToken
    )
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["GITHUB_REF_TYPE"] = "non-tag",
                    ["GITHUB_REF_NAME"] = "test",
                    ["GITHUB_EVENT_NAME"] = "test",
                    ["ACTIONS_ID_TOKEN_REQUEST_URL"] = oidcRequestUrl,
                    ["ACTIONS_ID_TOKEN_REQUEST_TOKEN"] = oidcRequestToken,
                }
            )
            .Build();

        var services = new ServiceCollection();
        services.AddGitHubSettings(config);

        await using var provider = services.BuildServiceProvider();

        Should.NotThrow(() => provider.GetRequiredService<IStartupValidator>().Validate());
    }

    [Test]
    public async Task AddGitHubSettingsShouldNotThrowIfAllRequiredValuesArePresent()
    {
        const string refType = "ref_type";
        const string refName = "ref_name";
        const string eventName = "event_name";

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["GITHUB_REF_TYPE"] = refType,
                    ["GITHUB_REF_NAME"] = refName,
                    ["GITHUB_EVENT_NAME"] = eventName,
                }
            )
            .Build();

        var services = new ServiceCollection();
        services.AddGitHubSettings(config);

        await using var provider = services.BuildServiceProvider();
        var validator = provider.GetRequiredService<IStartupValidator>();
        validator.Validate();

        var settings = provider.GetRequiredService<IOptions<GitHubSettings>>().Value;
        settings.RefType.ShouldBe(refType);
        settings.RefName.ShouldBe(refName);
        settings.EventName.ShouldBe(eventName);
    }
}
