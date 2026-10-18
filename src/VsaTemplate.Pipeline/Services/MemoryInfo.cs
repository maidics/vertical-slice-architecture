using System.Globalization;
using Microsoft.Extensions.Logging;
using VsaTemplate.Pipeline.Extensions;
using VsaTemplate.Pipeline.Interfaces;

namespace VsaTemplate.Pipeline.Services;

public sealed class MemoryInfo : IMemoryInfo
{
    private const long BytesPerKilobyte = 1024;

    private readonly string _memInfoPath;
    private bool _hasLogged;

    // memInfoPath is not registered in DI, so the container falls back to the default value
    public MemoryInfo(string memInfoPath = "/proc/meminfo")
    {
        _memInfoPath = memInfoPath;
    }

    public void LogWarningIfUsageIsGreaterThan(ILogger logger, double percent = 80)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(percent);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(percent, 100);

        if (_hasLogged)
            return;

        if (!File.Exists(_memInfoPath))
        {
            logger.LogError(
                "Memory info file not found at {Path}, memory usage cannot be checked.",
                _memInfoPath
            );

            _hasLogged = true;
            return;
        }

        // read on every call: GC.GetGCMemoryInfo() is only refreshed by this process' collections
        var (total, available) = ReadMemInfo();

        if (total <= 0 || available < 0)
        {
            logger.LogError(
                "Memory info file at {Path} has no valid MemTotal or MemAvailable entry, memory usage cannot be checked.",
                _memInfoPath
            );

            _hasLogged = true;
            return;
        }

        var used = total - available;
        var usage = used * 100d / total;

        if (usage <= percent)
            return;

        logger.LogWarning(
            "High memory usage: {Used} of {Total} in use ({Usage:0.##}%), above {Percent}%. Pipeline process working set: {WorkingSet}.",
            used.ToReadableBytes(),
            total.ToReadableBytes(),
            usage,
            percent,
            Environment.WorkingSet.ToReadableBytes()
        );

        _hasLogged = true;
    }

    private (long Total, long Available) ReadMemInfo()
    {
        long total = -1;
        long available = -1;

        // lines look like "MemTotal:        7911580 kB"
        foreach (var line in File.ReadLines(_memInfoPath))
        {
            if (line.StartsWith("MemTotal:", StringComparison.Ordinal))
                total = ParseKilobytes(line);
            else if (line.StartsWith("MemAvailable:", StringComparison.Ordinal))
                available = ParseKilobytes(line);

            if (total >= 0 && available >= 0)
                break;
        }

        return (total, available);
    }

    private static long ParseKilobytes(string line)
    {
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return
            parts.Length >= 2
            && long.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var kb)
            ? kb * BytesPerKilobyte
            : -1;
    }
}
