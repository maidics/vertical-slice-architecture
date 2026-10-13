using ModularPipelines;
using ModularPipelines.Options;
using Shouldly;
using VsaTemplate.Pipeline.Extensions;

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
}
