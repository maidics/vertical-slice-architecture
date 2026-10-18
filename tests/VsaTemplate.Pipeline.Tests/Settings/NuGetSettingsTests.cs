using System.Reflection;
using ModularPipelines.Attributes;
using Shouldly;
using VsaTemplate.Pipeline.Settings;

namespace VsaTemplate.Pipeline.Tests.Settings;

public sealed class NuGetSettingsTests
{
    [Test]
    public void UserShouldBeMarkedAsSecretValue()
    {
        typeof(NuGetSettings)
            .GetProperty(nameof(NuGetSettings.User))
            .ShouldNotBeNull()
            .GetCustomAttribute<SecretValueAttribute>()
            .ShouldNotBeNull();
    }
}
