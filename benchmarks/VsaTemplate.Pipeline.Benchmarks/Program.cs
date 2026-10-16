using VsaTemplate.Pipeline.Benchmarks.BenchmarkCases;
using VsaTemplate.Pipeline.Benchmarks.Services;

var benchmarkCase = Instantiations.Sequential;

await using var container = new TemplateBenchmarkContainer();
await container.InitializeAsync(benchmarkCase);

var result = await container.EvaluateAsync();

Console.WriteLine(result.ToString());
