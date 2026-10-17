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

[DependsOn<RestoreVariantsModule>]
public sealed class BuildVariantsModule : Module
{
    private readonly GitHubSettings _settings;

    public BuildVariantsModule(IOptions<GitHubSettings> options)
    {
        _settings = options.Value;
    }

    protected override ModuleConfiguration Configure()
    {
        return ModuleConfiguration.Create().WithNonPrAndNonMainPushSkip(_settings).Build();
    }

    protected override async Task ExecuteModuleAsync(
        IModuleContext context,
        CancellationToken cancellationToken
    )
    {
        await using var processor = BuildProcessor(
            TemplateVariant.All,
            context.DotNet(),
            cancellationToken
        );

        await processor.GetResultsAsync();
    }

    public static IAsyncProcessor<CommandResult> BuildProcessor(
        IReadOnlyList<TemplateVariant> variants,
        IDotNet dotnet,
        CancellationToken cancellationToken
    )
    {
        return variants
            .ToAsyncProcessorBuilder()
            .SelectAsync(v =>
                dotnet.Build(
                    new DotNetBuildOptions
                    {
                        NoRestore = true,
                        ProjectSolution = Path.Combine(
                            Template.ArtifactsDirectory.TemplateVariantsDirectory,
                            v.Name,
                            v.Name + ".slnx"
                        ),
                        Configuration = Template.Configuration,
                    },
                    cancellationToken: cancellationToken
                )
            )
            .ProcessInParallel(2);
    }
}
