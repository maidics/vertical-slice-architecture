using ModularPipelines;
using ModularPipelines.Extensions;
using ModularPipelines.Options;
using VsaTemplate.Pipeline.Modules;

namespace VsaTemplate.Pipeline.Extensions;

public static class PipelineBuilderExtensions
{
    extension(PipelineBuilder builder)
    {
        public PipelineBuilder ConfigurePipeline()
        {
            return builder.ConfigurePipelineOptions(options =>
            {
                options.PrintLogo = false;
                options.ShowProgressInConsole = true;
                options.ExecutionMode = ExecutionMode.StopOnFirstException;
            });
        }

        public PipelineBuilder AddModules()
        {
            return builder.AddModule<RestoreTemplateModule>().AddModule<BuildTemplateModule>();
        }
    }
}
