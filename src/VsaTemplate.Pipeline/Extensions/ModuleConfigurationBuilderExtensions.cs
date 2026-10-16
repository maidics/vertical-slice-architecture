using ModularPipelines.Configuration;
using ModularPipelines.Models;
using VsaTemplate.Pipeline.Settings;

namespace VsaTemplate.Pipeline.Extensions;

public static class ModuleConfigurationBuilderExtensions
{
    extension(ModuleConfigurationBuilder builder)
    {
        public ModuleConfigurationBuilder WithNonPrAndNonMainPushSkip(GitHubSettings settings) =>
            builder.WithSkipWhen(_ =>
                settings.IsPullRequest || settings.IsMainPush
                    ? SkipDecision.DoNotSkip
                    : SkipDecision.Skip("Only runs on PR or main push.")
            );
    }
}
