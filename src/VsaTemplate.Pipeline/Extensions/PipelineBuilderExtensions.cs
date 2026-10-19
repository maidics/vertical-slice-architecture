using Microsoft.Extensions.DependencyInjection;
using ModularPipelines;
using ModularPipelines.Extensions;
using ModularPipelines.Options;
using ModularPipelines.Requirements;
using VsaTemplate.Pipeline.Interfaces;
using VsaTemplate.Pipeline.Modules;
using VsaTemplate.Pipeline.Requirements;
using VsaTemplate.Pipeline.Services;

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

        public PipelineBuilder AddSettings()
        {
            builder
                .Services.AddGitHubSettings(builder.Configuration)
                .AddNuGetSettings(builder.Configuration);

            return builder;
        }

        public PipelineBuilder AddServices()
        {
            builder.Services.AddSingleton<ISolutionFilterBuilder, SolutionFilterBuilder>();
            builder.Services.AddTransient<IMemoryInfo, MemoryInfo>();
            builder.Services.AddSingleton<IGithubSecretMasker, GithubSecretMasker>();

            return builder;
        }

        public PipelineBuilder AddRequirements()
        {
            return builder
                .AddRequirement<LinuxRequirement>()
                .AddRequirement<VersionTagRequirement>();
        }

        public PipelineBuilder AddModules()
        {
            return builder
                .AddModule<RestoreTemplateModule>()
                .AddModule<BuildTemplateModule>()
                .AddModule<TestTemplateModule>()
                .AddModule<PackTemplateModule>()
                .AddModule<InstallTemplateModule>()
                .AddModule<InstantiateTemplateVariantsModule>()
                .AddModule<RestoreVariantsModule>()
                .AddModule<BuildVariantsModule>()
                .AddModule<TestVariantsModule>()
                .AddModule<GitHubOidcModule>()
                .AddModule<NuGetLoginModule>()
                .AddModule<NuGetPushModule>();
        }
    }
}
