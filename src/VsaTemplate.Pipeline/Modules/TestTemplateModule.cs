using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.DotNet.Services;
using ModularPipelines.Modules;
using VsaTemplate.Pipeline.Constants;
using VsaTemplate.Pipeline.Interfaces;

namespace VsaTemplate.Pipeline.Modules;

[DependsOn<BuildTemplateModule>]
public sealed class TestTemplateModule(ISolutionFilterBuilder builder) : Module
{
    protected override async Task ExecuteModuleAsync(
        IModuleContext context,
        CancellationToken cancellationToken
    )
    {
        var filter = await CreateSolutionFilterAsync(builder);

        await TestAsync(filter, context.DotNet(), cancellationToken);
    }

    public static async Task<string> CreateSolutionFilterAsync(ISolutionFilterBuilder builder)
    {
        return await builder
            .WithName(nameof(TestTemplateModule))
            .WithProjects(
                Template.ProjectPaths.UnitTests,
                Template.ProjectPaths.WebTests,
                Template.ProjectPaths.PipelineTests
            )
            .BuildAsync();
    }

    public static async Task TestAsync(
        string solutionFilter,
        IDotNet dotnet,
        CancellationToken cancellationToken
    )
    {
        var options = new DotNetTestOptions
        {
            NoRestore = true,
            NoBuild = true,
            Solution = solutionFilter,
            Configuration = Template.Configuration,
        };

        await dotnet.Test(options, cancellationToken: cancellationToken);
    }
}
