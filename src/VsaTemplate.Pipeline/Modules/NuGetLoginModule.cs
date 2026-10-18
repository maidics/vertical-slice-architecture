using Microsoft.Extensions.Options;
using ModularPipelines.Attributes;
using ModularPipelines.Configuration;
using ModularPipelines.Context;
using ModularPipelines.Modules;
using VsaTemplate.Pipeline.Extensions;
using VsaTemplate.Pipeline.Settings;

namespace VsaTemplate.Pipeline.Modules;

[DependsOn<TestVariantsModule>]
public sealed class NuGetLoginModule : Module
{
    private readonly GitHubSettings _gitHubSettings;
    private readonly NuGetSettings _nuGetSettings;

    public NuGetLoginModule(
        IOptions<GitHubSettings> githubOptions,
        IOptions<NuGetSettings> nugetOptions
    )
    {
        _gitHubSettings = githubOptions.Value;
        _nuGetSettings = nugetOptions.Value;
    }

    protected override ModuleConfiguration Configure()
    {
        return ModuleConfiguration.Create().WithNonTagPushSkip(_gitHubSettings).Build();
    }

    protected override Task ExecuteModuleAsync(
        IModuleContext context,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
    }
}
