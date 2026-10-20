using Microsoft.Extensions.Options;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Options;
using ModularPipelines.DotNet.Services;
using Shouldly;
using VsaTemplate.Pipeline.Constants;
using VsaTemplate.Pipeline.Modules;
using VsaTemplate.Pipeline.Settings;
using VsaTemplate.Pipeline.Tests.TestInfrastructure.BaseClasses;
using VsaTemplate.Pipeline.Tests.TestInfrastructure.Extensions;

namespace VsaTemplate.Pipeline.Tests.Modules;

[InheritsTests]
public sealed class NuGetPushModuleTests()
    : ModuleTestBase<NuGetPushModule>(typeof(NuGetLoginModule))
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

        var module = new NuGetPushModule(options);

        var config = module.GetConfiguration();
        var decision = await config.SkipCondition.ShouldNotBeNull().Invoke(null!);
        decision.ShouldSkip.ShouldBe(SkipDecisions.TagPush(options.Value).Invoke(null!).ShouldSkip);
    }

    [Test]
    public async Task PushPackageAsyncShouldPushPackageWithoutLoggingCommandArguments()
    {
        // DotNetNuget is a concrete class, so verify what it passes to the underlying command
        var command = ICommand.Mock();

        var dotnet = IDotNet.Mock();
        dotnet.Nuget.Returns(new DotNetNuget(command.Object));

        string apiKey = Guid.NewGuid().ToString();

        await NuGetPushModule.PushPackageAsync(apiKey, dotnet.Object, CancellationToken.None);

        command
            .ExecuteCommandLineTool(
                o =>
                    o is DotNetNugetPushOptions push
                    && push.ApiKey == apiKey
                    && push.Source == NuGetSettings.PushSource
                    && push.Path
                        == Path.Combine(
                            Template.ArtifactsDirectory.PackageReleaseDirectory,
                            "*.nupkg"
                        ),
                e => e != null && e.LogSettings != null && !e.LogSettings.ShowCommandArguments,
                Any()
            )
            .WasCalled(Times.Once);
    }
}
