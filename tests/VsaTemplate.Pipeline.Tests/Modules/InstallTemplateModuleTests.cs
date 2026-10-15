using ModularPipelines.Context;
using ModularPipelines.DotNet.Services;
using VsaTemplate.Pipeline.Constants;
using VsaTemplate.Pipeline.Modules;
using VsaTemplate.Pipeline.Tests.TestInfrastructure.BaseClasses;

namespace VsaTemplate.Pipeline.Tests.Modules;

[InheritsTests]
public sealed class InstallTemplateModuleTests()
    : ModuleTestBase<InstallTemplateModule>(typeof(PackTemplateModule))
{
    [Test]
    public async Task InstallAsyncShouldInstallWithCorrectOptions()
    {
        var dotnet = IDotNet.Mock();

        var dotnetNew = DotNetNew.Mock(ICommand.Mock());
        dotnet.New.Returns(dotnetNew);

        await InstallTemplateModule.InstallAsync(dotnet.Object, CancellationToken.None);

        dotnetNew
            .Execute(
                o =>
                    o.Arguments is not null
                    && o.Arguments.Count() == 2
                    && o.Arguments.First() == "install"
                    && o.Arguments.Last()
                        == Path.Combine(
                            Template.ArtifactsDirectory.PackageReleaseDirectory,
                            "*.nupkg"
                        ),
                Any(),
                Any()
            )
            .WasCalled(Times.Once);
    }
}
