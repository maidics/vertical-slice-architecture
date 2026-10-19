using ModularPipelines;
#if (TemplateDev)
using VsaTemplate.Pipeline.Extensions;
#endif

var builder = Pipeline.CreateBuilder(args);

// Build your pipeline here
// https://thomhurst.github.io/ModularPipelines/docs/
#if (TemplateDev)
builder.ConfigurePipeline().AddSettings().AddServices().AddRequirements().AddModules();
#endif

await using var pipeline = builder.Build();
await pipeline.RunAsync();
