using System.Diagnostics;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using TomLonghurst.EnumerableAsyncProcessor.Extensions;
using VsaTemplate.Pipeline.Benchmarks.Models;

namespace VsaTemplate.Pipeline.Benchmarks.Services;

public sealed class TemplateBenchmarkContainer : IAsyncDisposable
{
    private readonly TemplateBenchmarkCase _benchmarkCase;
    private readonly IContainer _container;

    public TemplateBenchmarkContainer(TemplateBenchmarkCase benchmarkCase)
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
    }

    public async Task<TemplateBenchmarkResult> EvaluateAsync()
    {
        await _container.StartAsync();

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
        var timer = new Stopwatch();
        timer.Start();

        var processor = step
            .CommandLineArguments.ToAsyncProcessorBuilder()
            .SelectAsync(arguments => _container.ExecAsync([.. arguments]))
            .ProcessInParallel(step.MaxConcurrency);

        var elapsed = timer.Elapsed;
        timer.Stop();

        ExecResult[] results = await processor.GetResultsAsync();

        Console.WriteLine(step.Name + " results:");

        foreach (var result in results)
        {
            Console.WriteLine($"Exit code: {result.ExitCode}");
            Console.WriteLine("-----------------------------");
            Console.WriteLine($"Stdout: {result.Stdout}");
            Console.WriteLine("-----------------------------");
            Console.WriteLine($"Stderr: {result.Stderr}");
        }

        throw new NotImplementedException();
    }

    public async ValueTask DisposeAsync()
    {
        await _container.DisposeAsync();
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
