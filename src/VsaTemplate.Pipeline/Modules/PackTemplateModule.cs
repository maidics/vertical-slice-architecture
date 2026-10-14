using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.DotNet.Services;
using ModularPipelines.Modules;
using VsaTemplate.Pipeline.Constants;

namespace VsaTemplate.Pipeline.Modules;

[DependsOn<TestTemplateModule>]
public sealed class PackTemplateModule : Module<string>
{
    protected override async Task<string?> ExecuteAsync(
        IModuleContext context,
        CancellationToken cancellationToken
    )
    {
        await PackAsync(context.DotNet(), cancellationToken);

        return EnsureSingleNupkg();
    }

    public static async Task PackAsync(IDotNet dotnet, CancellationToken cancellationToken)
    {
        var options = new DotNetPackOptions
        { // NoBuild is set in nuspec.csproj - passing --no-build would also skip restore
            ProjectSolution = Template.NuSpec,
            Configuration = Template.Configuration,
        };

        await dotnet.Pack(options, cancellationToken: cancellationToken);
    }

    public static string EnsureSingleNupkg()
    {
        var nupkgFiles = Directory
            .EnumerateFiles(Template.ArtifactsDirectory.PackOutput)
            .Where(f => Path.GetExtension(f) == ".nupkg")
            .ToList();

        if (nupkgFiles.Count == 0)
            throw new FileNotFoundException(
                $"No '.nupkg' file found in pack output folder: '{Template.ArtifactsDirectory.PackOutput}'."
            );

        if (nupkgFiles.Count > 1)
            throw new InvalidOperationException(
                $"More than one '.nupkg' files found in pack output folder ({Template.ArtifactsDirectory.PackOutput}): {string.Join(", ", nupkgFiles)}."
            );

        return nupkgFiles[0];
    }
}
