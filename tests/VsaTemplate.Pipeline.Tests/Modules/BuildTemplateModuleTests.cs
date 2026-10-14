using ModularPipelines.DotNet.Services;
using VsaTemplate.Pipeline.Constants;
using VsaTemplate.Pipeline.Modules;

namespace VsaTemplate.Pipeline.Tests.Modules;

public sealed class BuildTemplateModuleTests
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
