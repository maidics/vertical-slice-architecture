using System.Text;
using VsaTemplate.Pipeline.Interfaces;

namespace VsaTemplate.Pipeline.Services;

// ModularPipelines v3.2.8 leaks long JWTs via SecretValueAttribute and ISecretRegistry
// this implementation uses GitHub ::add-mask:: to hide secret values from the logs
public sealed class GithubSecretMasker : IGithubSecretMasker
{
    private readonly TextWriter _stdout;
    private readonly Lock _lock = new();

    public GithubSecretMasker()
        : this(
#pragma warning disable ConsoleUse
            new StreamWriter(Console.OpenStandardOutput(), new UTF8Encoding(false))
#pragma warning restore ConsoleUse
            {
                AutoFlush = true,
            }
        ) { }

    public GithubSecretMasker(TextWriter stdout) => _stdout = stdout;

    public void Mask(string secret)
    {
        if (string.IsNullOrEmpty(secret))
            return;

        lock (_lock)
        {
            _stdout.Write($"\n::add-mask::{Escape(secret)}\n");
        }
    }

    public static string Escape(string value) =>
        value.Replace("%", "%25").Replace("\r", "%0D").Replace("\n", "%0A");
}
