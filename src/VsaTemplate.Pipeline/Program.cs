using ModularPipelines;
using VsaTemplate.Pipeline.Extensions;

var builder = Pipeline.CreateBuilder(args);

// Build your pipeline here
// https://thomhurst.github.io/ModularPipelines/docs/
#if (TemplateDev)
builder.ConfigurePipeline().AddServices().AddModules();
#endif

await using var pipeline = builder.Build();
await pipeline.RunAsync();
