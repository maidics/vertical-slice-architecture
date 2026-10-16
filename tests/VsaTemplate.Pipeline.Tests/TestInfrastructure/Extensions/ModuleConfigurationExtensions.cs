using ModularPipelines.Configuration;
using ModularPipelines.Context;
using Shouldly;

namespace VsaTemplate.Pipeline.Tests.TestInfrastructure.Extensions;

public static class ModuleConfigurationExtensions
{
    extension(ModuleConfiguration config)
    {
        public async Task ShouldSkipOnNonPrAndNonMainPushAsync(
            bool isPrOrMainPush,
            IModuleContext? moduleContext = null
        )
        {
            config.SkipCondition.ShouldNotBeNull();
            var result = await config.SkipCondition.Invoke(moduleContext!);
            result.Reason.ShouldBe(isPrOrMainPush ? null : "Only runs on PR or main push.");
            result.ShouldSkip.ShouldBe(!isPrOrMainPush);
        }
    }
}
