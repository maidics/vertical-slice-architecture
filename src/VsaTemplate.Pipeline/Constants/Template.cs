namespace VsaTemplate.Pipeline.Constants;

public sealed class Template
{
    public const string Solution = nameof(VsaTemplate) + ".slnx";

    public const string Configuration = "Release";

    public sealed class ProjectPaths
    {
        public static string PipelineTests =>
            Path.Combine(
                "tests",
                "VsaTemplate.Pipeline.Tests",
                "VsaTemplate.Pipeline.Tests.csproj"
            );

        public static string UnitTests =>
            Path.Combine("tests", "VsaTemplate.UnitTests", "VsaTemplate.UnitTests.csproj");

        public static string WebTests =>
            Path.Combine("tests", "VsaTemplate.WebTests", "VsaTemplate.WebTests.csproj");
    }

    public sealed class ArtifactsDirectory
    {
        public static string SolutionFilterDirectory = Path.Combine(
            "artifacts",
            "solution-filters"
        );
    }
}
