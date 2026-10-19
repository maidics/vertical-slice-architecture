using Microsoft.Extensions.Logging;

namespace VsaTemplate.Pipeline.Interfaces;

public interface IMemoryInfo
{
    void LogWarningIfUsageIsGreaterThan(ILogger logger, double percent = 80);
}
