using EnumerableAsyncProcessor.Extensions;
using EnumerableAsyncProcessor.Interfaces;
using Microsoft.Extensions.Options;
using ModularPipelines.Attributes;
using ModularPipelines.Configuration;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.DotNet.Services;
using ModularPipelines.Models;
using ModularPipelines.Modules;
using VsaTemplate.Pipeline.Constants;
using VsaTemplate.Pipeline.Extensions;
using VsaTemplate.Pipeline.Models;
using VsaTemplate.Pipeline.Settings;

namespace VsaTemplate.Pipeline.Modules;

[DependsOn<InstantiateTemplateVariantsModule>]
public sealed class RestoreVariantsModule : Module
{
    private readonly GitHubSettings _settings;

    public RestoreVariantsModule(IOptions<GitHubSettings> options)
    {
        _settings = options.Value;
    }

    protected override ModuleConfiguration Configure()
    {
        return ModuleConfiguration
            .Create()
            .WithSkipWhen(SkipDecisions.PullRequestOrMainPushOrTagPush(_settings))
            .Build();
    }

    protected override async Task ExecuteModuleAsync(
        IModuleContext context,
        CancellationToken cancellationToken
    )
    {
        await using var processor = BuildAsyncProcessor(
            TemplateVariant.All,
            context.DotNet(),
            cancellationToken
        );

        await processor.GetResultsAsync();
    }

    public static IAsyncProcessor<CommandResult> BuildAsyncProcessor(
        IReadOnlyList<TemplateVariant> variants,
        IDotNet dotnet,
        CancellationToken cancellationToken
    ) =>
        variants
            .ToAsyncProcessorBuilder()
            .SelectAsync(v =>
                dotnet.Restore(
                    new DotNetRestoreOptions
                    {
                        ProjectSolution = Path.Combine(
                            Template.ArtifactsDirectory.TemplateVariantsDirectory,
                            v.Name,
                            v.Name + ".slnx"
                        ),
                    },
                    cancellationToken: cancellationToken
                )
            )
            .ProcessInParallel(maxConcurrency: 2);
}
