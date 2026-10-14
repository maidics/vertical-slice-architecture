using ModularPipelines.DotNet.Services;
using VsaTemplate.Pipeline.Constants;
using VsaTemplate.Pipeline.Modules;
using VsaTemplate.Pipeline.Tests.TestInfrastructure.BaseClasses;

namespace VsaTemplate.Pipeline.Tests.Modules;

[InheritsTests]
public sealed class RestoreTemplateModuleTests : ModuleTestBase<RestoreTemplateModule>
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
