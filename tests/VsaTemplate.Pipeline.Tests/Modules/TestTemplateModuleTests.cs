using ModularPipelines.DotNet.Services;
using VsaTemplate.Pipeline.Constants;
using VsaTemplate.Pipeline.Modules;
using VsaTemplate.Pipeline.Tests.TestInfrastructure.BaseClasses;

namespace VsaTemplate.Pipeline.Tests.Modules;

[InheritsTests]
public sealed class TestTemplateModuleTests()
    : ModuleTestBase<TestTemplateModule>(typeof(BuildTemplateModule))
{
    [Test]
    public async Task TestAsyncShouldTestWithCorrectOptions()
    {
        var dotnet = IDotNet.Mock();

        await TestTemplateModule.TestAsync(dotnet.Object, CancellationToken.None);

        dotnet
            .Test(
                o =>
                    o.NoRestore == true
                    && o.NoBuild == true
                    && o.Project == Template.PipelineTestProjectPath
                    && o.Configuration == Template.Configuration,
                Any(),
                Any()
            )
            .WasCalled(Times.Once);
    }
}
