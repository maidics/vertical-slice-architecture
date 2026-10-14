using ModularPipelines.DotNet.Services;
using Shouldly;
using VsaTemplate.Pipeline.Constants;
using VsaTemplate.Pipeline.Modules;
using VsaTemplate.Pipeline.Tests.TestInfrastructure.BaseClasses;
using VsaTemplate.Pipeline.Tests.TestInfrastructure.Services;

namespace VsaTemplate.Pipeline.Tests.Modules;

[InheritsTests]
public sealed class PackTemplateModuleTests()
    : ModuleTestBase<PackTemplateModule>(typeof(TestTemplateModule))
{
    private const string _ensureSingleNupkgTest = "Ensure Single .nupkg Test";

    [Test]
    public async Task PackAsyncShouldPackWithCorrectOptions()
    {
        var dotnet = IDotNet.Mock();

        await PackTemplateModule.PackAsync(dotnet.Object, CancellationToken.None);

        dotnet
            .Pack(
                o =>
                    o.NoRestore == true
                    && o.NoBuild == true
                    && o.ProjectSolution == Template.NuSpec
                    && o.Configuration == Template.Configuration,
                Any(),
                Any()
            )
            .WasCalled(Times.Once);
    }

    [Test]
    [NotInParallel(_ensureSingleNupkgTest)]
    public async Task EnsureSingleNupkgShouldThrowIfNoNupkgFound()
    {
        await using var fileSystem = new TestFileSystem(Template.ArtifactsDirectory.PackOutput);

        var ex = Should.Throw<FileNotFoundException>(PackTemplateModule.EnsureSingleNupkg);
        ex.Message.ShouldBe(
            $"No '.nupkg' file found in pack output folder: '{Template.ArtifactsDirectory.PackOutput}'."
        );
    }

    [Test]
    [NotInParallel(_ensureSingleNupkgTest)]
    public async Task EnsureSingleNupkgShouldThrowIfMoreThanOneNupkgExists()
    {
        await using var fileSystem = new TestFileSystem(Template.ArtifactsDirectory.PackOutput);
        var file1 = fileSystem.CreateTestFile("package1.nupkg");
        var file2 = fileSystem.CreateTestFile("package2.nupkg");

        var file1Relative = Path.GetRelativePath(Environment.CurrentDirectory, file1);
        var file2Relative = Path.GetRelativePath(Environment.CurrentDirectory, file2);

        var ex = Should.Throw<InvalidOperationException>(PackTemplateModule.EnsureSingleNupkg);
        ex.Message.ShouldBe(
            $"More than one '.nupkg' files found in pack output folder ({Template.ArtifactsDirectory.PackOutput}): {string.Join(", ", file1Relative, file2Relative)}."
        );
    }

    [Test]
    [NotInParallel(_ensureSingleNupkgTest)]
    public async Task EnsureSingleNupkgShouldReturnSingleNupkg()
    {
        await using var fileSystem = new TestFileSystem(Template.ArtifactsDirectory.PackOutput);

        var nupkgFile = fileSystem.CreateTestFile("package.nupkg");
        var relative = Path.GetRelativePath(Environment.CurrentDirectory, nupkgFile);

        fileSystem.CreateTestFile("package.json");

        var nupkg = PackTemplateModule.EnsureSingleNupkg();
        nupkg.ShouldBe(relative);
    }
}
