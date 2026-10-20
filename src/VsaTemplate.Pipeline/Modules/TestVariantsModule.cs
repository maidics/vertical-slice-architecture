using EnumerableAsyncProcessor.Extensions;
using EnumerableAsyncProcessor.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModularPipelines.Attributes;
using ModularPipelines.Configuration;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.DotNet.Services;
using ModularPipelines.Logging;
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
    private readonly ISolutionFilterBuilder _solutionFilterBuilder;
    private readonly IMemoryInfo _memoryInfo;

    public TestVariantsModule(
        IOptions<GitHubSettings> options,
        ISolutionFilterBuilder solutionFilterBuilder,
        IMemoryInfo memoryInfo
    )
    {
        _memoryInfo = memoryInfo;
        _solutionFilterBuilder = solutionFilterBuilder;
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
        var filters = await CreateSolutionFilters(_solutionFilterBuilder, TemplateVariant.All);

        await using var processor = BuildAsyncProcessor(
            filters,
            context.DotNet(),
            _memoryInfo,
            context.Logger,
            cancellationToken
        );

        await processor.GetResultsAsync();
    }

    public static async Task<IReadOnlyList<string>> CreateSolutionFilters(
        ISolutionFilterBuilder builder,
        IReadOnlyList<TemplateVariant> variants
    )
    {
        var filters = new List<string>();

        foreach (var variant in variants)
        {
            var filter = await builder
                .WithName($"Variant.{variant.Name}")
                .WithSolution(variant.Solution)
                .WithProjects([.. variant.TestProjects])
                .BuildAsync();

            filters.Add(filter);
        }

        return filters;
    }

    public static IAsyncProcessor<CommandResult> BuildAsyncProcessor(
        IReadOnlyList<string> solutionFilters,
        IDotNet dotnet,
        IMemoryInfo memoryInfo,
        ILogger logger,
        CancellationToken cancellationToken
    )
    {
        if (solutionFilters.Any(f => !File.Exists(f)))
            throw new FileNotFoundException(
                $"Solution filter(s) not found: {string.Join(", ", solutionFilters.Where(f => !File.Exists(f)))}."
            );

        return solutionFilters
            .ToAsyncProcessorBuilder()
            .SelectAsync(async f =>
            {
                var result = await dotnet.Test(
                    new DotNetTestOptions
                    {
                        NoRestore = true,
                        NoBuild = true,
                        Configuration = Template.Configuration,
                        Solution = f,
                    },
                    cancellationToken: cancellationToken
                );

                memoryInfo.LogWarningIfUsageIsGreaterThan(logger);

                return result;
            })
            .ProcessInParallel(maxConcurrency: int.MaxValue);
    }
}
