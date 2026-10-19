using System.Text.Json;
using VsaTemplate.Pipeline.Constants;
using VsaTemplate.Pipeline.Interfaces;

namespace VsaTemplate.Pipeline.Services;

public sealed class SolutionFilterBuilder : ISolutionFilterBuilder
{
    private string _name = string.Empty;
    private string _solution = string.Empty;
    private HashSet<string> _projects = new(StringComparer.Ordinal);

    public ISolutionFilterBuilder WithName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Solution Filter name cannot be null or white space.",
                nameof(name)
            );

        _name = name;

        return this;
    }

    public ISolutionFilterBuilder WithSolution(string solution)
    {
        if (!File.Exists(solution))
            throw new FileNotFoundException($"Solution not found: {solution}.");

        if (Path.GetExtension(solution) is not (".slnx" or ".sln"))
            throw new NotSupportedException(
                $"Passed file is not a '.slnx' or '.sln' file: {solution}."
            );

        _solution = solution;

        return this;
    }

    public ISolutionFilterBuilder WithProjects(params string[] projects)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(projects.Length, 0);

        var notFound = projects.Where(p => !File.Exists(p)).ToList();

        if (notFound.Count > 0)
            throw new FileNotFoundException(
                $"Project(s) not found: {string.Join(", ", notFound)}."
            );

        var nonCsproj = projects.Where(p => Path.GetExtension(p) != ".csproj").ToList();

        if (nonCsproj.Count > 0)
            throw new NotSupportedException(
                $"Passed files are not '.csproj' files: {string.Join(", ", nonCsproj)}."
            );

        _projects = projects.ToHashSet();

        return this;
    }

    public async Task<string> BuildAsync()
    {
        if (string.IsNullOrWhiteSpace(_name))
            throw new InvalidOperationException("Solution Filter name has not been set.");

        if (string.IsNullOrWhiteSpace(_solution))
            throw new InvalidOperationException("Solution Filter solution has not been set.");

        if (_projects.Count == 0)
            throw new InvalidOperationException("Solution Filter projects has not been set.");

        // slnf: solution path is relative to the filter, project paths are relative to the solution
        var solutionDirectory = Path.GetDirectoryName(Path.GetFullPath(_solution))!;

        var content = new
        {
            solution = new
            {
                path = Path.GetRelativePath(
                    Template.ArtifactsDirectory.SolutionFilterDirectory,
                    _solution
                ),
                projects = _projects.Select(p => Path.GetRelativePath(solutionDirectory, p)),
            },
        };

        var serialized = JsonSerializer.Serialize(
            content,
            new JsonSerializerOptions { WriteIndented = true }
        );

        Directory.CreateDirectory(Template.ArtifactsDirectory.SolutionFilterDirectory);

        var filter = Path.Combine(
            Template.ArtifactsDirectory.SolutionFilterDirectory,
            $"VsaTemplate.{_name}.slnf"
        );

        await File.WriteAllTextAsync(filter, serialized);

        return filter;
    }
}
