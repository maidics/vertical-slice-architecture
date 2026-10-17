using Microsoft.Extensions.Options;
using ModularPipelines.Context;
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
public sealed class InstantiateTemplateVariantsModuleTests()
    : ModuleTestBase<InstantiateTemplateVariantsModule>(typeof(InstallTemplateModule))
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

        var module = new InstantiateTemplateVariantsModule(options);
        var config = module.GetConfiguration();
        await config.ShouldSkipOnNonPrAndNonMainPushAsync(
            options.Value.IsPullRequest || options.Value.IsMainPush
        );
    }

    [Test]
    public async Task BuildAsyncProcessorShouldBuildProcessor()
    {
        var dotnet = IDotNet.Mock();
        var dotnetNew = DotNetNew.Mock(ICommand.Mock());
        dotnet.New.Returns(dotnetNew);

        var variants = TemplateVariant.All;

        var processor = InstantiateTemplateVariantsModule.BuildAsyncProcessor(
            variants,
            dotnet,
            CancellationToken.None
        );

        var tasks = processor.GetEnumerableTasks().ToList();
        tasks.Count.ShouldBe(variants.Count);
        processor.GetMaxConcurrency().ShouldBe(Environment.ProcessorCount);

        await processor.GetResultsAsync();

        foreach (var variant in variants)
        {
            List<string> expectedArgs =
            [
                "vsa-sln",
                "-n",
                variant.Name,
                "-o",
                Path.Combine(Template.ArtifactsDirectory.TemplateVariantsDirectory, variant.Name),
                .. variant.Arguments,
            ];

            dotnetNew
                .Execute(
                    o => o.Arguments is not null && o.Arguments.SequenceEqual(expectedArgs),
                    Any(),
                    Any()
                )
                .WasCalled(Times.Once);
        }
    }
}
