namespace VsaTemplate.Pipeline.Constants;

public static class Template
{
    public const string Solution = nameof(VsaTemplate) + ".slnx";

    public const string Configuration = "Release";

    public const string NuSpec = "nuspec.csproj";

    public static class ProjectPaths
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

    public static class ArtifactsDirectory
    {
        public static readonly string SolutionFilterDirectory = Path.Combine(
            "artifacts",
            "solution-filters"
        );

        public static readonly string PackageReleaseDirectory = Path.Combine(
            "artifacts",
            "package",
            "release"
        );

        public static readonly string TemplateVariantsDirectory = Path.Combine(
            "artifacts",
            "template-variants"
        );
    }
}
