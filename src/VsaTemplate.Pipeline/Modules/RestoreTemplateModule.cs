using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.DotNet.Services;
using ModularPipelines.Modules;
using VsaTemplate.Pipeline.Constants;

namespace VsaTemplate.Pipeline.Modules;

public sealed class RestoreTemplateModule : Module
{
    protected override async Task ExecuteModuleAsync(
        IModuleContext context,
        CancellationToken cancellationToken
    )
    {
        await RestoreAsync(context.DotNet(), cancellationToken);
    }

    public static async Task RestoreAsync(IDotNet dotnet, CancellationToken cancellationToken)
    {
        var options = new DotNetRestoreOptions { ProjectSolution = Template.Solution };

        await dotnet.Restore(options, cancellationToken: cancellationToken);
    }
}
