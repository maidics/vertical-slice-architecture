namespace VsaTemplate.Pipeline.Tests.TestInfrastructure.Constants;

public sealed class TemplateTesting
{
    public static string ResolvePathFromSolutionRoot(string path)
    {
        return Path.Combine("..", "..", "..", "..", path);
    }
}
