namespace VsaTemplate.Pipeline.Constants;

public sealed class Template
{
    public const string Solution = nameof(VsaTemplate) + ".slnx";

    public const string Configuration = "Release";

    public static string PipelineTestProjectPath =>
        Path.Combine("tests", "VsaTemplate.Pipeline.Tests", "VsaTemplate.Pipeline.Tests.csproj");
}
