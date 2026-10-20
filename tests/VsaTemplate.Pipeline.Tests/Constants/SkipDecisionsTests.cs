using Shouldly;
using VsaTemplate.Pipeline.Constants;
using VsaTemplate.Pipeline.Settings;

namespace VsaTemplate.Pipeline.Tests.Constants;

public sealed class SkipDecisionsTests
{
    [Test]
    [Arguments("pull_request", "branch", "42/merge", false)]
    [Arguments("push", "branch", "main", false)]
    [Arguments("push", "branch", "feat/pgsql-db-option", true)]
    // Branch names that only look like main
    [Arguments("push", "branch", "release/main", true)]
    [Arguments("push", "branch", "main-old", true)]
    [Arguments("push", "branch", "Main", true)] // git refs are case-sensitive
    // Tag pushes
    [Arguments("push", "tag", "v1.0.0", false)]
    // pull_request_target runs on the default branch's ref
    [Arguments("pull_request_target", "branch", "main", true)]
    // Non-push events on main
    [Arguments("workflow_dispatch", "branch", "main", true)]
    [Arguments("schedule", "branch", "main", true)]
    [Arguments("merge_group", "branch", "gh-readonly-queue/main/pr-42-abc123", true)]
    public void PullRequestOrMainPushOrTagPushShouldSkipCorrectly(
        string eventName,
        string refType,
        string refName,
        bool shouldSkip
    )
    {
        var settings = new GitHubSettings
        {
            RefType = refType,
            RefName = refName,
            EventName = eventName,
            OidcRequestUrl = "test",
            OidcRequestToken = "test",
        };

        var func = SkipDecisions.PullRequestOrMainPushOrTagPush(settings);

        var decision = func.Invoke(null!);
        decision.ShouldSkip.ShouldBe(shouldSkip);

        if (shouldSkip)
            decision.Reason.ShouldBe(SkipDecisions.Messages.PullRequestOrMainPushOrTagPush);
    }

    [Test]
    [Arguments("tag", false)]
    [Arguments("non-tag", true)]
    public void TagPushShouldSkipCorrectly(string refType, bool shouldSkip)
    {
        var settings = new GitHubSettings
        {
            RefType = refType,
            RefName = "test",
            EventName = "test",
            OidcRequestUrl = "test",
            OidcRequestToken = "test",
        };

        var func = SkipDecisions.TagPush(settings);

        var decision = func.Invoke(null!);
        decision.ShouldSkip.ShouldBe(shouldSkip);

        if (shouldSkip)
            decision.Reason.ShouldBe(SkipDecisions.Messages.TagPush);
    }
}
