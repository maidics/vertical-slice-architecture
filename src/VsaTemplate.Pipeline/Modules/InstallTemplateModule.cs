using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.DotNet.Services;
using ModularPipelines.Modules;
using VsaTemplate.Pipeline.Constants;

namespace VsaTemplate.Pipeline.Modules;

[DependsOn<PackTemplateModule>]
public sealed class InstallTemplateModule : Module
{
    protected override async Task ExecuteModuleAsync(
        IModuleContext context,
        CancellationToken cancellationToken
    )
    {
        await InstallAsync(context.DotNet(), cancellationToken);
    }

    public static async Task InstallAsync(IDotNet dotnet, CancellationToken cancellationToken)
    {
        var options = new DotNetNewOptions
        {
            Arguments =
            [
                "install",
                Path.Combine(Template.ArtifactsDirectory.PackageReleaseDirectory, "*.nupkg"),
            ],
        };

        await dotnet.New.Execute(options, cancellationToken: cancellationToken);
    }
}
