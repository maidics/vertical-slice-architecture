using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.DotNet.Services;
using ModularPipelines.Modules;
using VsaTemplate.Pipeline.Constants;
using VsaTemplate.Pipeline.Modules;
using VsaTemplate.Pipeline.Settings;

namespace VsaTemplate.Pipeline.Tests.Modules;

[DependsOn<NuGetLoginModule>]
public sealed class NuGetPushModule : Module
{
    protected override async Task ExecuteModuleAsync(
        IModuleContext context,
        CancellationToken cancellationToken
    )
    {
        var result = await context.GetModule<NuGetLoginModule>();
        ArgumentNullException.ThrowIfNull(result.ValueOrDefault);

        await PushPackageAsync(result.ValueOrDefault, context.DotNet(), cancellationToken);
    }

    public static async Task PushPackageAsync(
        string apiKey,
        IDotNet dotnet,
        CancellationToken cancellationToken
    )
    {
        // PackTemplateModule ensures only one package exists
        var options = new DotNetNugetPushOptions
        {
            Path = Path.Combine(Template.ArtifactsDirectory.PackageReleaseDirectory, "*.nupkg"),
            ApiKey = apiKey,
            Source = NuGetSettings.PushSource,
        };

        await dotnet.Nuget.Push(options, cancellationToken: cancellationToken);
    }
}
