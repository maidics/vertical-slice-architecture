using ModularPipelines.DotNet.Services;
using VsaTemplate.Pipeline.Constants;
using VsaTemplate.Pipeline.Interfaces;
using VsaTemplate.Pipeline.Modules;
using VsaTemplate.Pipeline.Tests.TestInfrastructure.BaseClasses;

namespace VsaTemplate.Pipeline.Tests.Modules;

[InheritsTests]
public sealed class TestTemplateModuleTests()
    : ModuleTestBase<TestTemplateModule>(typeof(BuildTemplateModule))
{
    [Test]
    public async Task BuildSolutionFilterAsyncShouldBuildCorrectFilter()
    {
        var builder = ISolutionFilterBuilder.Mock();
        builder.WithName(Any()).Returns(builder.Object);
        builder.WithProjects(Any()).Returns(builder.Object);

        await TestTemplateModule.CreateSolutionFilterAsync(builder);

        builder.WithName(nameof(TestTemplateModule)).WasCalled(Times.Once);
        builder
            .WithProjects(
                Template.ProjectPaths.UnitTests,
                Template.ProjectPaths.WebTests,
                Template.ProjectPaths.PipelineTests
            )
            .WasCalled(Times.Once);
        builder.BuildAsync().WasCalled(Times.Once);
    }

    [Test]
    public async Task TestAsyncShouldTestWithCorrectOptions()
    {
        const string filter = nameof(filter);
        var dotnet = IDotNet.Mock();

        await TestTemplateModule.TestAsync(filter, dotnet.Object, CancellationToken.None);

        dotnet
            .Test(
                o =>
                    o.NoRestore == true
                    && o.NoBuild == true
                    && o.Solution == filter
                    && o.Configuration == Template.Configuration,
                Any(),
                Any()
            )
            .WasCalled(Times.Once);
    }
}
