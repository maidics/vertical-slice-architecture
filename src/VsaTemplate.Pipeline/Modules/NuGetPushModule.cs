using Microsoft.Extensions.Options;
using ModularPipelines.Attributes;
using ModularPipelines.Configuration;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.DotNet.Services;
using ModularPipelines.Modules;
using ModularPipelines.Options;
using VsaTemplate.Pipeline.Constants;
using VsaTemplate.Pipeline.Extensions;
using VsaTemplate.Pipeline.Settings;

namespace VsaTemplate.Pipeline.Modules;

[DependsOn<NuGetLoginModule>]
public sealed class NuGetPushModule : Module
{
    private readonly GitHubSettings _settings;

    public NuGetPushModule(IOptions<GitHubSettings> options)
    {
        _settings = options.Value;
    }

    protected override ModuleConfiguration Configure()
    {
        return ModuleConfiguration.Create().WithSkipWhen(SkipDecisions.TagPush(_settings)).Build();
    }

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

        await dotnet.Nuget.Push(
            options,
            new CommandExecutionOptions
            {
                LogSettings = new CommandLoggingOptions { ShowCommandArguments = false }, // api key is passed as -k so logging is disabled on this
            },
            cancellationToken: cancellationToken
        );
    }
}
