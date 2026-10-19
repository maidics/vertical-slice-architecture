using Microsoft.Extensions.Options;
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
        var githubOptions = Options.Create(
            new GitHubSettings
            {
                RefType = refType,
                RefName = "test",
                EventName = "test",
                OidcRequestUrl = "test",
                OidcRequestToken = "test",
            }
        );

        var nugetOptions = IOptions<NuGetSettings>.Mock();

        var module = new NuGetLoginModule(githubOptions, nugetOptions.Object);

        var config = module.GetConfiguration();
        await config.ShouldSkipOnNonTagPushAsync(githubOptions.Value.IsTagPush);
    }

    // TODO: test static methods, register module via extension and test registration
}
