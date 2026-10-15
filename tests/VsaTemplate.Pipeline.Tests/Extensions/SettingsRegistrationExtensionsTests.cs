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
    [MethodDataSource(nameof(AddGitHubSettingsShouldThrowOnStartIfAnyConfigurationIsEmptySource))]
    public async Task AddGitHubSettingsShouldThrowOnStartIfAnyConfigurationIsEmpty(
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
