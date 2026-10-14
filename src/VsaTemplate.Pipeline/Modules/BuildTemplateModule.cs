using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.DotNet.Services;
using ModularPipelines.Modules;
using VsaTemplate.Pipeline.Constants;

namespace VsaTemplate.Pipeline.Modules;

[DependsOn<RestoreTemplateModule>]
public sealed class BuildTemplateModule : Module
{
    protected override async Task ExecuteModuleAsync(
        IModuleContext context,
        CancellationToken cancellationToken
    )
    {
        await BuildAsync(context.DotNet(), cancellationToken);
    }

    public static async Task BuildAsync(IDotNet dotnet, CancellationToken cancellationToken)
    {
        var options = new DotNetBuildOptions
        {
            ProjectSolution = Template.Solution,
            NoRestore = true,
            Configuration = Template.Configuration,
        };

        await dotnet.Build(options, cancellationToken: cancellationToken);
    }
}
