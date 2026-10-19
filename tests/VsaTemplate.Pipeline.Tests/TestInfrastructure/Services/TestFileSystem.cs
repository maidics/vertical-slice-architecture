namespace VsaTemplate.Pipeline.Tests.TestInfrastructure.Services;

public sealed class TestFileSystem : IAsyncDisposable
{
    public string TempDir { get; }

    public TestFileSystem()
    {
        TempDir = Directory.CreateTempSubdirectory("vsa-template-tests-").FullName;
    }

    public TestFileSystem(string tempDir)
    {
        var directory = Path.GetFullPath(tempDir);

        Directory.CreateDirectory(directory);

        TempDir = directory;
    }

    public ValueTask DisposeAsync()
    {
        try
        {
            Directory.Delete(TempDir, recursive: true);
        }
        catch (Exception ex)
        {
#pragma warning disable ConsoleUse
            Console.WriteLine($"Failed to delete temp test folder: {ex.Message} at \"{TempDir}\".");
#pragma warning restore ConsoleUse
        }

        return ValueTask.CompletedTask;
    }

    public string CreateTestFile(string? name = null)
    {
        var path = Path.Combine(TempDir, name ?? Guid.NewGuid().ToString("N"));
        File.Create(path).Dispose();
        return path;
    }

    public string CreateSubdirectory(string? name = null)
    {
        var path = Path.Combine(TempDir, name ?? Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
