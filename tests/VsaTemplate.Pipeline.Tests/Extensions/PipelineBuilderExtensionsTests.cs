using Microsoft.Extensions.DependencyInjection;
using ModularPipelines;
using ModularPipelines.Modules;
using ModularPipelines.Options;
using ModularPipelines.Requirements;
using Shouldly;
using VsaTemplate.Pipeline.Extensions;
using VsaTemplate.Pipeline.Interfaces;
using VsaTemplate.Pipeline.Modules;
using VsaTemplate.Pipeline.Services;

namespace VsaTemplate.Pipeline.Tests.Extensions;

public sealed class PipelineBuilderExtensionsTests
{
    private readonly PipelineBuilder _builder = ModularPipelines.Pipeline.CreateBuilder();

    [Test]
    public void ConfigurePipelineShouldConfigurePipeline()
    {
        _builder.ConfigurePipeline();

        _builder.Options.PrintLogo.ShouldBeFalse();
        _builder.Options.ShowProgressInConsole.ShouldBeTrue();
        _builder.Options.ExecutionMode.ShouldBe(ExecutionMode.StopOnFirstException);
    }

    [Test]
    public async Task ShouldAddAllModulesAndServices()
    {
        _builder.AddServices();
        _builder.AddModules();

        List<Type> expected =
        [
            typeof(RestoreTemplateModule),
            typeof(BuildTemplateModule),
            typeof(TestTemplateModule),
            typeof(PackTemplateModule),
            typeof(InstallTemplateModule),
            typeof(InstantiateTemplateVariantsModule),
            typeof(RestoreVariantsModule),
            typeof(BuildVariantsModule),
            typeof(TestVariantsModule),
            typeof(GitHubOidcModule),
        ];

        await using var pipeline = await _builder.BuildAsync();

        pipeline.Services.GetServices<IModule>().Select(m => m.GetType()).ShouldBe(expected);

        pipeline.Services.GetService<ISolutionFilterBuilder>().ShouldNotBeNull();
        pipeline.Services.GetService<IMemoryInfo>().ShouldNotBeNull();

        ContainsService<ISolutionFilterBuilder, SolutionFilterBuilder>(ServiceLifetime.Singleton)
            .ShouldBeTrue();
        ContainsService<IMemoryInfo, MemoryInfo>(ServiceLifetime.Transient).ShouldBeTrue();
    }

    [Test]
    public void ShouldAddRequirements()
    {
        _builder.AddRequirements();

        _builder.Services.Count(d => d.ServiceType == typeof(IPipelineRequirement)).ShouldBe(1);

        ContainsService<IPipelineRequirement, LinuxRequirement>().ShouldBeTrue();
    }

    private bool ContainsService<TService, TImplementation>(ServiceLifetime? lifetime = null)
        where TService : notnull
        where TImplementation : TService =>
        _builder.Services.Any(d =>
            d.ServiceType == typeof(TService)
            && d.ImplementationType == typeof(TImplementation)
            && (lifetime is null || d.Lifetime == lifetime)
        );
}
