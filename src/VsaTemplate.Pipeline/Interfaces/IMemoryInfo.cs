namespace VsaTemplate.Pipeline.Interfaces;

public interface IMemoryInfo
{
    void LogWarningIfUsageIsGreater(double percent = 80);
}
