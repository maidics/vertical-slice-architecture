namespace VsaTemplate.Pipeline.Benchmarks.Models;

public sealed record TemplateBenchmarkMaxConcurrencyOptions
{
    public int Instantiation;
    public int Restore;
    public int Build;
    public int Test;

    public TemplateBenchmarkMaxConcurrencyOptions(
        int instantiation = 1,
        int restore = 1,
        int build = 1,
        int test = 1
    )
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(instantiation, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(restore, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(build, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(test, 0);

        Instantiation = instantiation;
        Restore = restore;
        Build = build;
        Test = test;
    }
}
