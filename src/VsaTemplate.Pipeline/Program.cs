using ModularPipelines;
using ModularPipelines.Extensions;
using VsaTemplate.Pipeline.Extensions;

var builder = Pipeline.CreateBuilder(args);

// Build your pipeline here
// https://thomhurst.github.io/ModularPipelines/docs/
#if (TemplateDev)
builder.ConfigurePipeline().AddModules();
#endif

await using var pipeline = builder.Build();
await pipeline.RunAsync();
