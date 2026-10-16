using ModularPipelines.Context;
using ModularPipelines.DotNet.Services;
using Shouldly;
using VsaTemplate.Pipeline.Constants;
using VsaTemplate.Pipeline.Models;
using VsaTemplate.Pipeline.Modules;
using VsaTemplate.Pipeline.Tests.TestInfrastructure.BaseClasses;

namespace VsaTemplate.Pipeline.Tests.Modules;

[InheritsTests]
public sealed class InstantiateTemplateVariantsModuleTests()
    : ModuleTestBase<InstantiateTemplateVariantsModule>(typeof(InstallTemplateModule))
{
    [Test]
    public async Task BuildAsyncProcessorShouldBuildProcessor()
    {
        var dotnet = IDotNet.Mock();
        var dotnetNew = DotNetNew.Mock(ICommand.Mock());
        dotnet.New.Returns(dotnetNew);

        var variants = TemplateVariant.All;

        var processor = InstantiateTemplateVariantsModule.BuildAsyncProcessor(variants, dotnet);

        var tasks = processor.GetEnumerableTasks().ToList();
        tasks.Count.ShouldBe(variants.Count);

        await processor.GetResultsAsync();

        foreach (var variant in variants)
        {
            List<string> expectedArgs =
            [
                "vsa-sln",
                "-n",
                variant.Name,
                "-o",
                Path.Combine(Template.ArtifactsDirectory.TemplateVariantsDirectory, variant.Name),
                .. variant.Arguments,
            ];

            dotnetNew
                .Execute(
                    o =>
                        o.Arguments is not null && Equals(o.Arguments, expectedArgs.AsReadOnly()),
                    Any(),
                    Any()
                )
                .WasCalled(Times.Once);
        }
    }
}
