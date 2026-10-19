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
    public void WithSolutionShouldThrowIfSolutionFileIsNotFound()
    {
        var invalid = Guid.NewGuid() + ".slnx";

        var ex = Should.Throw<FileNotFoundException>(() => _builder.WithSolution(invalid));
        ex.Message.ShouldBe($"Solution not found: {invalid}.");
    }

    [Test]
    public void WithSolutionShouldThrowIfSolutionIsNotSlnxOrSln()
    {
        var project = TemplateTesting.ResolvePathFromSolutionRoot(
            Template.ProjectPaths.PipelineTests
        );

        var ex = Should.Throw<NotSupportedException>(() => _builder.WithSolution(project));
        ex.Message.ShouldBe($"Passed file is not a '.slnx' or '.sln' file: {project}.");
    }

    [Test]
    public void WithProjectsShouldThrowIfProjectCountIsZero()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => _builder.WithProjects());
    }

    [Test]
    public void WithProjectsShouldThrowIfAnyProjectFileIsNotFound()
    {
        var invalid1 = Guid.NewGuid().ToString();
        var invalid2 = Guid.NewGuid().ToString();

        string[] projects =
        [
            invalid1,
            invalid2,
            TemplateTesting.ResolvePathFromSolutionRoot(Template.ProjectPaths.PipelineTests),
        ];

        var ex = Should.Throw<FileNotFoundException>(() => _builder.WithProjects(projects));
        ex.Message.ShouldContain("Project(s) not found:");
        ex.Message.ShouldContain(invalid1);
        ex.Message.ShouldContain(invalid2);
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
    public async Task BuildAsyncShouldThrowWhenSolutionIsNotSet()
    {
        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            _builder.WithName("test").BuildAsync()
        );
        ex.Message.ShouldBe("Solution Filter solution has not been set.");
    }

    [Test]
    public async Task BuildAsyncShouldThrowWhenProjectsIsNotSet()
    {
        var solution = TemplateTesting.ResolvePathFromSolutionRoot(Template.Solution);

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            _builder.WithName("test").WithSolution(solution).BuildAsync()
        );
        ex.Message.ShouldBe("Solution Filter projects has not been set.");
    }

    [Test]
    public async Task BuildAsyncShouldCreateSolutionFilterFile()
    {
        await using var fileSystem = new TestFileSystem("artifacts");

        const string name = "SolutionFilterBuilderTesting";
        var solutionPath = TemplateTesting.ResolvePathFromSolutionRoot(Template.Solution);
        string[] prjcts =
        [
            TemplateTesting.ResolvePathFromSolutionRoot(Template.ProjectPaths.PipelineTests),
        ];

        var filter = await _builder
            .WithName(name)
            .WithSolution(solutionPath)
            .WithProjects(prjcts)
            .BuildAsync();

        File.Exists(filter).ShouldBeTrue();
        Path.GetFileName(filter).ShouldBe($"VsaTemplate.{name}.slnf");

        await using var stream = File.OpenRead(filter);
        using var doc = await JsonDocument.ParseAsync(stream);

        var solution = doc.RootElement.GetProperty("solution");

        var path = solution.GetProperty("path");
        path.ValueKind.ShouldBe(JsonValueKind.String);
        Path.GetFullPath(Path.Combine(Path.GetDirectoryName(filter)!, path.GetString()!))
            .ShouldBe(Path.GetFullPath(solutionPath));

        var projects = solution.GetProperty("projects");
        projects.ValueKind.ShouldBe(JsonValueKind.Array);
        projects.GetArrayLength().ShouldBe(1);
        projects
            .EnumerateArray()
            .Select(p => p.GetString())
            .ShouldBe([Template.ProjectPaths.PipelineTests]);
    }
}
