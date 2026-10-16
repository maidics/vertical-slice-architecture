namespace VsaTemplate.Pipeline.Benchmarks.Models;

public sealed record TemplateBenchmarkCommandResult(
    string Command,
    long? ExitCode,
    string Stdout,
    string Stderr
)
{
    public override string ToString()
    {
        return $"""
               Command: {Command}
                    - ExitCode: {ExitCode}
                    - Stdout: {Stdout}
                    - Stderr: {Stderr} 
            """;
    }
}
