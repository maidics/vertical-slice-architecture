using System.Diagnostics;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using TomLonghurst.EnumerableAsyncProcessor.Extensions;
using VsaTemplate.Pipeline.Benchmarks.Models;

namespace VsaTemplate.Pipeline.Benchmarks.Services;

public sealed class TemplateBenchmarkContainer : IAsyncDisposable
{
    private TemplateBenchmarkCase _benchmarkCase = null!;
    private IContainer _container = null!;
    private ContainerMemoryPeak _memoryPeak = null!;
    private ContainerStorageUsage _storageUsage = null!;

    public async Task InitializeAsync(TemplateBenchmarkCase benchmarkCase)
    {
        _benchmarkCase = benchmarkCase;

        _container = new ContainerBuilder(HostContainer.Image)
            .WithHostname(benchmarkCase.Name)
            .WithCreateParameterModifier(p =>
            {
                p.HostConfig ??= new HostConfig();
                p.HostConfig.NanoCPUs = HostContainer.MaxCpu;
                p.HostConfig.Memory = HostContainer.MaxMemory;
                p.HostConfig.StorageOpt = new Dictionary<string, string>
                {
                    ["size"] = HostContainer.Storage,
                };
            })
            .WithCommand("sleep", "infinity")
            .WithEnvironment("DOTNET_CLI_TELEMETRY_OPTOUT", "1")
            .WithEnvironment("DOTNET_NOLOGO", "1")
            .WithWaitStrategy(Wait.ForUnixContainer())
            .Build();

        await _container.StartAsync();
        await InstallTemplateAsync();

        _memoryPeak = new ContainerMemoryPeak(_container.Id);
        _storageUsage = new ContainerStorageUsage(_container.Id);
    }

    private async Task InstallTemplateAsync()
    {
        const string template = "Vertical.Slice.Architecture";

        Console.WriteLine($"Installing {template}...");

        var result = await _container.ExecAsync(["dotnet", "new", "install", template]);

        Console.WriteLine($"Template installation finished with exit code: {result.ExitCode}");
        Console.WriteLine($"\t- Stdout: \n\t\t{result.Stdout}");
        Console.WriteLine($"\t- Stderr: \n\t\t{result.Stderr}");

        if (result.ExitCode is not null && result.ExitCode != 0)
            throw new InvalidOperationException();
    }

    public async Task<TemplateBenchmarkResult> EvaluateAsync()
    {
        var stepResults = new List<TemplateBenchmarkStepResult>();

        foreach (var step in _benchmarkCase.Steps)
        {
            var result = await EvaluateStepAsync(step);
            stepResults.Add(result);
        }

        return new TemplateBenchmarkResult(_benchmarkCase.Name, stepResults);
    }

    private async Task<TemplateBenchmarkStepResult> EvaluateStepAsync(TemplateBenchmarkStep step)
    {
        _memoryPeak.Reset();

        var timer = new Stopwatch();
        timer.Start();

        var processor = step
            .CommandLineArguments.ToAsyncProcessorBuilder()
            .SelectAsync(arguments => _container.ExecAsync([.. arguments]))
            .ProcessInParallel(step.MaxConcurrency);

        ExecResult[] results = await processor.GetResultsAsync();

        var elapsed = timer.Elapsed;
        timer.Stop();

        var commandResults = results
            .Select(
                (r, i) =>
                    new TemplateBenchmarkCommandResult(
                        string.Join(" ", step.CommandLineArguments[i]),
                        r.ExitCode,
                        r.Stdout,
                        r.Stderr
                    )
            )
            .ToList();

        var writableBytes = await _storageUsage.GetWritableLayerBytesAsync();

        return new TemplateBenchmarkStepResult(
            step.Name,
            elapsed,
            _memoryPeak.ReadPeakBytes(),
            writableBytes ?? -1,
            commandResults
        );
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
        await _storageUsage.DisposeAsync();
    }

    private static class HostContainer
    {
        // this includes git
        public const string Image = "mcr.microsoft.com/dotnet/sdk:10.0.401";

        // GitHub Actions free tier, public repo
        public const long MaxCpu = 4_000_000_000;
        public const long MaxMemory = 16L * 1024 * 1024 * 1024;
        public const string Storage = "14G";
    }
}
