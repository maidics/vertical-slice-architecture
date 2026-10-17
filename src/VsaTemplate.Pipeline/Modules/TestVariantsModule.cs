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
using VsaTemplate.Pipeline.Interfaces;
using VsaTemplate.Pipeline.Models;
using VsaTemplate.Pipeline.Settings;

namespace VsaTemplate.Pipeline.Modules;

[DependsOn<BuildVariantsModule>]
public sealed class TestVariantsModule : Module
{
    private readonly GitHubSettings _settings;
    private readonly IMemoryInfo _memoryInfo;

    public TestVariantsModule(IOptions<GitHubSettings> options, IMemoryInfo memoryInfo)
    {
        _memoryInfo = memoryInfo;
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
            _memoryInfo,
            cancellationToken
        );

        await processor.GetResultsAsync();
    }

    public static IAsyncProcessor<CommandResult> BuildProcessor(
        IReadOnlyList<TemplateVariant> variants,
        IDotNet dotnet,
        IMemoryInfo memoryInfo,
        CancellationToken cancellationToken
    )
    {
        return variants
            .ToAsyncProcessorBuilder()
            .SelectAsync<CommandResult>(async v =>
            {
                var result = await dotnet.Test(
                    new DotNetTestOptions
                    {
                        NoRestore = true,
                        NoBuild = true,
                        Configuration = Template.Configuration,
                        Solution = Path.Combine(
                            Template.ArtifactsDirectory.TemplateVariantsDirectory,
                            v.Name,
                            v.Name + ".slnx"
                        ),
                    },
                    cancellationToken: cancellationToken
                );

                memoryInfo.LogWarningIfUsageIsGreater();

                return result;
            })
            .ProcessInParallel(maxConcurrency: Environment.ProcessorCount);
    }
}
