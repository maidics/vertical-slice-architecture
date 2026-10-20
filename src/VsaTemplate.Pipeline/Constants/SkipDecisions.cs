using ModularPipelines.Context;
using ModularPipelines.Models;
using VsaTemplate.Pipeline.Settings;

namespace VsaTemplate.Pipeline.Constants;

public static class SkipDecisions
{
    public static Func<IModuleContext, SkipDecision> PullRequestOrMainPushOrTagPush(
        GitHubSettings settings
    ) =>
        _ =>
            settings.IsPullRequest || settings.IsMainPush || settings.IsTagPush
                ? SkipDecision.DoNotSkip
                : SkipDecision.Skip("Only runs on pull request, main push or tag push.");

    public static Func<IModuleContext, SkipDecision> TagPush(GitHubSettings settings) =>
        _ =>
            settings.IsTagPush
                ? SkipDecision.DoNotSkip
                : SkipDecision.Skip("Only runs on tag push.");
}
