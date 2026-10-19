using Shouldly;
using VsaTemplate.Pipeline.Services;
using VsaTemplate.Pipeline.Tests.TestInfrastructure.Services;

namespace VsaTemplate.Pipeline.Tests.Services;

public sealed class GithubSecretMaskerTests
{
    [Test]
    [Arguments("secret", "secret")]
    [Arguments("%", "%25")]
    [Arguments("\r", "%0D")]
    [Arguments("\n", "%0A")]
    [Arguments("a%b\r\nc", "a%25b%0D%0Ac")]
    [Arguments("%0A", "%250A")] // % is escaped first, so already escaped data stays literal
    public void EscapeShouldEscapeWorkflowCommandData(string value, string expected)
    {
        GithubSecretMasker.Escape(value).ShouldBe(expected);
    }

    [Test]
    [Arguments(null)]
    [Arguments("")]
    public void MaskShouldNotWriteIfSecretIsNullOrEmpty(string? secret)
    {
        using var output = new StringWriter();

        new GithubSecretMasker(output).Mask(secret!);

        output.ToString().ShouldBeEmpty();
    }

    [Test]
    public void MaskShouldWriteLongSecretAsSingleCommandLine()
    {
        using var output = new StringWriter();
        var secret = new string('x', 4096);

        new GithubSecretMasker(output).Mask(secret);

        output.ToString().ShouldBe($"\n::add-mask::{secret}\n");
    }

    [Test]
    public void MaskShouldEscapeSecret()
    {
        using var output = new StringWriter();

        new GithubSecretMasker(output).Mask("first\r\nsecond%");

        output.ToString().ShouldBe("\n::add-mask::first%0D%0Asecond%25\n");
    }

    [Test]
    public void MaskShouldWriteOneCommandPerCall()
    {
        using var output = new StringWriter();
        var masker = new GithubSecretMasker(output);

        masker.Mask("first");
        masker.Mask("second");

        output.ToString().ShouldBe("\n::add-mask::first\n\n::add-mask::second\n");
    }

    [Test]
    public void MaskShouldNotInterleaveConcurrentWrites()
    {
        using var output = new CharByCharTextWriter();
        var masker = new GithubSecretMasker(output);

        var secrets = Enumerable
            .Range(0, 50)
            .Select(i => $"{i}-{string.Concat(Enumerable.Repeat(Guid.NewGuid(), 10))}")
            .ToList();

        const int threadCount = 8;
        using var barrier = new Barrier(threadCount); // start all writers at once to force overlap

        var threads = Enumerable
            .Range(0, threadCount)
            .Select(t => new Thread(() =>
            {
                barrier.SignalAndWait();

                foreach (var secret in secrets.Where((_, i) => i % threadCount == t))
                    masker.Mask(secret);
            }))
            .ToList();

        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join());

        output
            .ToString()
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .ShouldBe(secrets.Select(s => $"::add-mask::{s}"), ignoreOrder: true);
    }
}
