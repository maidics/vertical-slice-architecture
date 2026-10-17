using Microsoft.Extensions.Options;
using ModularPipelines.DotNet.Services;
using Shouldly;
using VsaTemplate.Pipeline.Constants;
using VsaTemplate.Pipeline.Models;
using VsaTemplate.Pipeline.Modules;
using VsaTemplate.Pipeline.Settings;
using VsaTemplate.Pipeline.Tests.TestInfrastructure.BaseClasses;
using VsaTemplate.Pipeline.Tests.TestInfrastructure.Extensions;

namespace VsaTemplate.Pipeline.Tests.Modules;

[InheritsTests]
public sealed class BuildVariantsModuleTests()
    : ModuleTestBase<BuildVariantsModule>(typeof(RestoreVariantsModule))
{
    [Test]
    [Arguments("pull_request", "branch", "42/merge")]
    [Arguments("push", "branch", "main")]
    [Arguments("push", "branch", "feat/pgsql-db-option")]
    [Arguments("push", "tag", "v1.0.0")]
    public async Task ShouldSkipOnNonPrAndNonMainPush(
        string eventName,
        string refType,
        string refName
    )
    {
        var options = Options.Create(
            new GitHubSettings
            {
                RefType = refType,
                RefName = refName,
                EventName = eventName,
            }
        );

        var module = new BuildVariantsModule(options);
        var config = module.GetConfiguration();
        await config.ShouldSkipOnNonPrAndNonMainPushAsync(
            options.Value.IsPullRequest || options.Value.IsMainPush
        );
    }

    [Test]
    public async Task BuildProcessorShouldBuildProcessor()
    {
        var dotnet = IDotNet.Mock();

        var variants = TemplateVariant.All;

        await using var processor = BuildVariantsModule.BuildProcessor(
            variants,
            dotnet,
            CancellationToken.None
        );

        var tasks = processor.GetEnumerableTasks().ToList();
        tasks.Count.ShouldBe(variants.Count);
        processor.GetMaxConcurrency().ShouldBe(2);

        await processor.GetResultsAsync();

        foreach (var variant in variants)
        {
            var expectedSolution = Path.Combine(
                Template.ArtifactsDirectory.TemplateVariantsDirectory,
                variant.Name,
                variant.Name + ".slnx"
            );

            dotnet
                .Build(
                    o =>
                        o.ProjectSolution == expectedSolution
                        && o.NoRestore == true
                        && o.Configuration == Template.Configuration,
                    Any(),
                    Any()
                )
                .WasCalled(Times.Once);
        }
    }
}
