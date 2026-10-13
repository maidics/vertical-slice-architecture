using ModularPipelines;

var builder = Pipeline.CreateBuilder(args);

// Build your pipeline here
// https://thomhurst.github.io/ModularPipelines/docs/
#if (TemplateDev)
// TODO
#endif

await using var pipeline = builder.Build();
await pipeline.RunAsync();
