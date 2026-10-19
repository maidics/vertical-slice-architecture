using ModularPipelines.Configuration;
using VsaTemplate.Pipeline.Extensions;
using VsaTemplate.Pipeline.Settings;
using VsaTemplate.Pipeline.Tests.TestInfrastructure.Extensions;

namespace VsaTemplate.Pipeline.Tests.Extensions;

public sealed class ModuleConfigurationBuilderExtensionsTests
{
    [Test]
    [Arguments("pull_request", "branch", "42/merge", true)]
    [Arguments("push", "branch", "main", true)]
    [Arguments("push", "branch", "feat/pgsql-db-option", false)]
    // Branch names that only look like main
    [Arguments("push", "branch", "release/main", false)]
    [Arguments("push", "branch", "main-old", false)]
    [Arguments("push", "branch", "Main", false)] // git refs are case-sensitive
    // Tag pushes
    [Arguments("push", "tag", "v1.0.0", false)]
    [Arguments("push", "tag", "main", false)] // tag named main
    // pull_request_target runs on the default branch's ref
    [Arguments("pull_request_target", "branch", "main", false)]
    // Non-push events on main
    [Arguments("workflow_dispatch", "branch", "main", false)]
    [Arguments("schedule", "branch", "main", false)]
    [Arguments("merge_group", "branch", "gh-readonly-queue/main/pr-42-abc123", false)]
    public async Task WithNonPrAndNonMainPushSkipShouldOnlyRunOnPrOrMainPush(
        string eventName,
        string refType,
        string refName,
        bool isPrOrMainPush
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

        var config = ModuleConfiguration.Create().WithNonPrAndNonMainPushSkip(settings).Build();

        await config.ShouldSkipOnNonPrAndNonMainPushAsync(isPrOrMainPush);
    }

    [Test]
    [Arguments("push", "tag", "v1.0.0", true)]
    [Arguments("push", "tag", "main", true)] // tag named main
    [Arguments("push", "branch", "main", false)]
    [Arguments("push", "branch", "v1.0.0", false)] // branch named like a tag
    [Arguments("pull_request", "branch", "42/merge", false)]
    // Non-push events on branches
    [Arguments("workflow_dispatch", "branch", "main", false)]
    [Arguments("schedule", "branch", "main", false)]
    [Arguments("merge_group", "branch", "gh-readonly-queue/main/pr-42-abc123", false)]
    public async Task WithNonTagPushSkipShouldOnlyRunOnTagPush(
        string eventName,
        string refType,
        string refName,
        bool isTagPush
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

        var config = ModuleConfiguration.Create().WithNonTagPushSkip(settings).Build();

        await config.ShouldSkipOnNonTagPushAsync(isTagPush);
    }
}
