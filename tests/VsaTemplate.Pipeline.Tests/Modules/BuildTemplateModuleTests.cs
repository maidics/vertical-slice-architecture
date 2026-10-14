using ModularPipelines.DotNet.Services;
using VsaTemplate.Pipeline.Constants;
using VsaTemplate.Pipeline.Modules;
using VsaTemplate.Pipeline.Tests.TestInfrastructure.BaseClasses;

namespace VsaTemplate.Pipeline.Tests.Modules;

[InheritsTests]
public sealed class BuildTemplateModuleTests()
    : ModuleTestBase<BuildTemplateModule>(typeof(RestoreTemplateModule))
{
    [Test]
    public async Task BuildAsyncShouldBuildWithCorrectOptions()
    {
        var dotnet = IDotNet.Mock();

        await BuildTemplateModule.BuildAsync(dotnet.Object, CancellationToken.None);

        dotnet
            .Build(
                o =>
                    o.ProjectSolution == Template.Solution
                    && o.NoRestore == true
                    && o.Configuration == Template.Configuration,
                Any(),
                Any()
            )
            .WasCalled(Times.Once);
    }
}
