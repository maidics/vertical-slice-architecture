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
                : SkipDecision.Skip(Messages.PullRequestOrMainPushOrTagPush);

    public static Func<IModuleContext, SkipDecision> TagPush(GitHubSettings settings) =>
        _ => settings.IsTagPush ? SkipDecision.DoNotSkip : SkipDecision.Skip(Messages.TagPush);

    public static class Messages // this exists just so requirements can reuse it for logs since requirements have no skip logic
    {
        // ReSharper disable once MemberHidesStaticFromOuterClass
        public const string PullRequestOrMainPushOrTagPush =
            "Only runs on pull request, main push or tag push.";

        // ReSharper disable once MemberHidesStaticFromOuterClass
        public const string TagPush = "Only runs on tag push.";
    }
}
