namespace VsaTemplate.Pipeline.Interfaces;

public interface ISolutionFilterBuilder
{
    ISolutionFilterBuilder WithName(string name);
    ISolutionFilterBuilder WithSolution(string solution);
    ISolutionFilterBuilder WithProjects(params string[] projects);
    Task<string> BuildAsync();
}
