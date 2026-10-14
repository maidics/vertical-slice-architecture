using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.DotNet.Services;
using ModularPipelines.Modules;
using VsaTemplate.Pipeline.Constants;

namespace VsaTemplate.Pipeline.Modules;

[DependsOn<BuildTemplateModule>]
public sealed class TestTemplateModule : Module
{
    protected override async Task ExecuteModuleAsync(
        IModuleContext context,
        CancellationToken cancellationToken
    )
    {
        await TestAsync(context.DotNet(), cancellationToken);
    }

    public static async Task TestAsync(IDotNet dotnet, CancellationToken cancellationToken)
    {
        var options = new DotNetTestOptions
        {
            NoRestore = true,
            NoBuild = true,
            Project = Path.Join(
                "test",
                "VsaTemplate.Pipeline.Tests",
                Template.PipelineTestProjectPath
            ),
            Configuration = Template.Configuration,
        };

        await dotnet.Test(options, cancellationToken: cancellationToken);
    }
}
