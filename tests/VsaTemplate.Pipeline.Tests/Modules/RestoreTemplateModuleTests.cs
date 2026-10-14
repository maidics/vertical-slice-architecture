using ModularPipelines.DotNet.Services;
using VsaTemplate.Pipeline.Constants;
using VsaTemplate.Pipeline.Modules;

namespace VsaTemplate.Pipeline.Tests.Modules;

public sealed class RestoreTemplateModuleTests
{
    [Test]
    public async Task RestoreAsyncShouldCallRestoreWithCorrectOptions()
    {
        var dotnet = IDotNet.Mock();

        await RestoreTemplateModule.RestoreAsync(dotnet.Object, CancellationToken.None);

        dotnet
            .Restore(o => o.ProjectSolution == Template.Solution, Any(), Any())
            .WasCalled(Times.Once);
    }
}
