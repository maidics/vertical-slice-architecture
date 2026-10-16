namespace VsaTemplate.Pipeline.Benchmarks.Services;

public sealed class ContainerMemoryPeak
{
    private readonly string _peakFile;

    public ContainerMemoryPeak(string containerId)
    {
        _peakFile = $"/sys/fs/cgroup/memory/docker/{containerId}/memory.max_usage_in_bytes";

        if (!File.Exists(_peakFile))
            throw new FileNotFoundException($"cgroup file not found: {_peakFile}.");
    }

    public void Reset() => File.WriteAllText(_peakFile, "0");

    public long ReadPeakBytes() => long.Parse(File.ReadAllText(_peakFile).Trim());
}
