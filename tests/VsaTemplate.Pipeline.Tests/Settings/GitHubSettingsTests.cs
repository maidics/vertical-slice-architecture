using Shouldly;
using VsaTemplate.Pipeline.Settings;

namespace VsaTemplate.Pipeline.Tests.Settings;

public sealed class GitHubSettingsTests
{
    [Test]
    [Arguments("pull_request", true)]
    [Arguments("pull_request_target", false)]
    [Arguments("push", false)]
    [Arguments("workflow_dispatch", false)]
    [Arguments("schedule", false)]
    [Arguments("merge_group", false)]
    public void IsPullRequestShouldOnlyReturnTrueForPullRequestEvent(
        string eventName,
        bool isPullRequest
    )
    {
        var settings = new GitHubSettings
        {
            RefType = "test",
            RefName = "test",
            EventName = eventName,
        };

        settings.IsPullRequest.ShouldBe(isPullRequest);
    }

    [Test]
    [Arguments("push", "branch", "main", true)]
    [Arguments("push", "branch", "feat/pgsql-db-option", false)]
    // Branch names that only look like main
    [Arguments("push", "branch", "release/main", false)]
    [Arguments("push", "branch", "main-old", false)]
    [Arguments("push", "branch", "Main", false)] // git refs are case-sensitive
    // Tag pushes
    [Arguments("push", "tag", "v1.0.0", false)]
    [Arguments("push", "tag", "main", false)] // tag named main
    // Pull requests: ref name is "<n>/merge"
    [Arguments("pull_request", "branch", "42/merge", false)]
    // pull_request_target runs on the default branch's ref
    [Arguments("pull_request_target", "branch", "main", false)]
    // Non-push events on main
    [Arguments("workflow_dispatch", "branch", "main", false)]
    [Arguments("schedule", "branch", "main", false)]
    [Arguments("merge_group", "branch", "gh-readonly-queue/main/pr-42-abc123", false)]
    public void IsMainPushShouldOnlyReturnTrueForMainPushEvent(
        string eventName,
        string refType,
        string refName,
        bool isMainPush
    )
    {
        var settings = new GitHubSettings
        {
            RefType = refType,
            RefName = refName,
            EventName = eventName,
        };

        settings.IsMainPush.ShouldBe(isMainPush);
    }
}
