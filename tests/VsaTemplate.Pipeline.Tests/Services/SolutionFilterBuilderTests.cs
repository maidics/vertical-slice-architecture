using System.Text.Json;
using Shouldly;
using VsaTemplate.Pipeline.Constants;
using VsaTemplate.Pipeline.Services;
using VsaTemplate.Pipeline.Tests.TestInfrastructure.Constants;
using VsaTemplate.Pipeline.Tests.TestInfrastructure.Services;

namespace VsaTemplate.Pipeline.Tests.Services;

public sealed class SolutionFilterBuilderTests
{
    private readonly SolutionFilterBuilder _builder = new();

    [Test]
    [Arguments(null)]
    public void WithNameShouldThrowIfNameIsNullOrWhiteSpace(string? name)
    {
        var ex = Should.Throw<ArgumentException>(() => _builder.WithName(name!));
        ex.Message.ShouldContain("Solution Filter name cannot be null or white space.");
    }

    [Test]
    public void WithProjectsShouldThrowIfProjectCountIsZero()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => _builder.WithProjects());
    }

    [Test]
    public void WithProjectsShouldThrowIfAnyProjectFileIsNotFound()
    {
        string[] projects =
        [
            "invalid1",
            "invalid2",
            TemplateTesting.ResolvePathFromSolutionRoot(Template.ProjectPaths.PipelineTests),
        ];

        var ex = Should.Throw<FileNotFoundException>(() => _builder.WithProjects(projects));
        ex.Message.ShouldBe($"Project(s) not found: {string.Join(", ", "invalid1", "invalid2")}.");
    }

    [Test]
    public void WithProjectsShouldThrowIfAnyProjectIsNotCsproj()
    {
        var solution = TemplateTesting.ResolvePathFromSolutionRoot(Template.Solution);

        string[] projects =
        [
            solution,
            TemplateTesting.ResolvePathFromSolutionRoot(Template.ProjectPaths.PipelineTests),
        ];

        var ex = Should.Throw<NotSupportedException>(() => _builder.WithProjects(projects));
        ex.Message.ShouldBe($"Passed files are not '.csproj' files: {solution}.");
    }

    [Test]
    public async Task BuildAsyncShouldThrowWhenNameIsNotSet()
    {
        var ex = await Should.ThrowAsync<InvalidOperationException>(() => _builder.BuildAsync());
        ex.Message.ShouldBe("Solution Filter name has not been set.");
    }

    [Test]
    public async Task BuildAsyncShouldThrowWhenProjectsIsNotSet()
    {
        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            _builder.WithName("test").BuildAsync()
        );
        ex.Message.ShouldBe("Solution Filter projects has not been set.");
    }

    [Test]
    public async Task BuildAsyncShouldCreateSolutionFilterFile()
    {
        await using var fileSystem = new TestFileSystem("artifacts");

        const string name = "SolutionFilterBuilderTesting";
        string[] prjcts =
        [
            TemplateTesting.ResolvePathFromSolutionRoot(Template.ProjectPaths.PipelineTests),
        ];

        var filter = await _builder.WithName(name).WithProjects(prjcts).BuildAsync();

        File.Exists(filter).ShouldBeTrue();
        Path.GetFileName(filter).ShouldBe($"VsaTemplate.{name}.slnf");

        await using var stream = File.OpenRead(filter);
        using var doc = await JsonDocument.ParseAsync(stream);

        var solution = doc.RootElement.GetProperty("solution");

        var path = solution.GetProperty("path");
        path.ValueKind.ShouldBe(JsonValueKind.String);
        path.GetString().ShouldBe(Path.Join("..", "..", Template.Solution));

        var projects = solution.GetProperty("projects");
        projects.ValueKind.ShouldBe(JsonValueKind.Array);
        projects.GetArrayLength().ShouldBe(1);
        projects.EnumerateArray().Select(p => p.GetString()).ShouldBe(prjcts);
    }
}
