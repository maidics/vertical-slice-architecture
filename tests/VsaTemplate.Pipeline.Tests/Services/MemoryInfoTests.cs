using Microsoft.Extensions.Logging;
using Shouldly;
using VsaTemplate.Pipeline.Services;
using Microsoft.Extensions.Logging.Testing;
using VsaTemplate.Pipeline.Tests.TestInfrastructure.Services;

namespace VsaTemplate.Pipeline.Tests.Services;

public sealed class MemoryInfoTests
{
    // 8 GB total, 2 GB available -> 75% used
    private const string MemInfo = """
        MemTotal:        8388608 kB
        MemFree:          524288 kB
        MemAvailable:    2097152 kB
        Buffers:           35188 kB
        """;

    private readonly FakeLogger _logger = new();

    [Test]
    [Arguments(0d)]
    [Arguments(-1d)]
    [Arguments(100.1d)]
    public void LogWarningIfUsageIsGreaterShouldThrowIfPercentIsOutOfRange(double percent)
    {
        var memoryInfo = new MemoryInfo();

        Should.Throw<ArgumentOutOfRangeException>(() =>
            memoryInfo.LogWarningIfUsageIsGreater(_logger, percent)
        );
    }

    [Test]
    public async Task LogWarningIfUsageIsGreaterShouldLogWarningIfUsageIsAbovePercent()
    {
        await using var fileSystem = new TestFileSystem();
        var memoryInfo = new MemoryInfo(CreateMemInfo(fileSystem));

        memoryInfo.LogWarningIfUsageIsGreater(_logger, 70);

        _logger.Collector.Count.ShouldBe(1);
        _logger.Collector.LatestRecord.Level.ShouldBe(LogLevel.Warning);
        _logger.Collector.LatestRecord.Message.ShouldStartWith(
            "High memory usage: 6 GB of 8 GB in use (75%), above 70%."
        );
        _logger.Collector.LatestRecord.Message.ShouldContain("Pipeline process working set: ");
    }

    [Test]
    public async Task LogWarningIfUsageIsGreaterShouldLogOnlyOnce()
    {
        await using var fileSystem = new TestFileSystem();
        var memoryInfo = new MemoryInfo(CreateMemInfo(fileSystem));

        memoryInfo.LogWarningIfUsageIsGreater(_logger, 70);
        memoryInfo.LogWarningIfUsageIsGreater(_logger, 70);

        _logger.Collector.Count.ShouldBe(1);
    }

    [Test]
    [Arguments(75d)]
    [Arguments(80d)]
    public async Task LogWarningIfUsageIsGreaterShouldNotLogIfUsageIsNotAbovePercent(
        double percent
    )
    {
        await using var fileSystem = new TestFileSystem();
        var memoryInfo = new MemoryInfo(CreateMemInfo(fileSystem));

        memoryInfo.LogWarningIfUsageIsGreater(_logger, percent);

        _logger.Collector.Count.ShouldBe(0);
    }

    [Test]
    [Arguments("")]
    [Arguments("MemAvailable:    2097152 kB")]
    [Arguments("MemTotal:        8388608 kB")]
    [Arguments("MemTotal:        abc kB\nMemAvailable:    2097152 kB")]
    [Arguments("MemTotal:        8388608 kB\nMemAvailable:    abc kB")]
    [Arguments("MemTotal:\nMemAvailable:    2097152 kB")]
    [Arguments("MemTotal:        8388608 kB\nMemAvailable:")]
    [Arguments("MemTotal:        -8388608 kB\nMemAvailable:    2097152 kB")]
    [Arguments("MemTotal:        8388608 kB\nMemAvailable:    -2097152 kB")]
    [Arguments("MemTotal:        0 kB\nMemAvailable:    2097152 kB")]
    public async Task LogWarningIfUsageIsGreaterShouldLogErrorOnceIfMemInfoIsInvalid(
        string content
    )
    {
        await using var fileSystem = new TestFileSystem();
        var path = fileSystem.CreateTestFile("meminfo");
        await File.WriteAllTextAsync(path, content);
        var memoryInfo = new MemoryInfo(path);

        memoryInfo.LogWarningIfUsageIsGreater(_logger, 1);
        memoryInfo.LogWarningIfUsageIsGreater(_logger, 1);

        _logger.Collector.Count.ShouldBe(1);
        _logger.Collector.LatestRecord.Level.ShouldBe(LogLevel.Error);
        _logger.Collector.LatestRecord.Message.ShouldBe(
            $"Memory info file at {path} has no valid MemTotal or MemAvailable entry, memory usage cannot be checked."
        );
    }

    [Test]
    public async Task LogWarningIfUsageIsGreaterShouldLogErrorOnceIfMemInfoIsNotFound()
    {
        await using var fileSystem = new TestFileSystem();
        var path = Path.Combine(fileSystem.TempDir, "meminfo");
        var memoryInfo = new MemoryInfo(path);

        memoryInfo.LogWarningIfUsageIsGreater(_logger);
        memoryInfo.LogWarningIfUsageIsGreater(_logger);

        _logger.Collector.Count.ShouldBe(1);
        _logger.Collector.LatestRecord.Level.ShouldBe(LogLevel.Error);
        _logger.Collector.LatestRecord.Message.ShouldBe(
            $"Memory info file not found at {path}, memory usage cannot be checked."
        );
    }

    private static string CreateMemInfo(TestFileSystem fileSystem)
    {
        var path = fileSystem.CreateTestFile("meminfo");
        File.WriteAllText(path, MemInfo);
        return path;
    }
}
