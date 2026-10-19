using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModularPipelines.Context;
using Shouldly;
using VsaTemplate.Pipeline.Requirements;
using VsaTemplate.Pipeline.Settings;
using VsaTemplate.Pipeline.Tests.TestInfrastructure.Services;

namespace VsaTemplate.Pipeline.Tests.Requirements;

public sealed class VersionTagRequirementTests
{
    [Test]
    public async Task ShouldReturnEarlyOnNonTagPush()
    {
        var options = CreateOptions("not-tag", "v1.0.0");

        var requirement = new VersionTagRequirement(options);

        var logger = new FakeModuleLogger();

        var context = IPipelineHookContext.Mock();
        context.Logger.Returns(logger);

        var result = await requirement.MustAsync(context.Object);
        result.Success.ShouldBeTrue();

        logger.Collector.Count.ShouldBe(1);
        logger.Collector.LatestRecord.Level.ShouldBe(LogLevel.Information);
        logger.Collector.LatestRecord.Message.ShouldBe(
            $"{typeof(VersionTagRequirement)} - returning early on non tag push."
        );
    }

    [Test]
    [Arguments("test")]
    [Arguments("1-1-1")]
    [Arguments("1_1_1")]
    [Arguments("1.1.1")]
    [Arguments("1.1.1.1")]
    [Arguments("v1.1.1.1")]
    [Arguments("v01.2.3")]
    [Arguments("v1.02.3")]
    [Arguments("v1.2.03")]
    [Arguments("v1.2.3-rc.1")]
    [Arguments("v1.2.3\n")]
    public async Task ShouldReturnFailedWhenTagIsNotValid(string refName)
    {
        var options = CreateOptions("tag", refName);

        var requirement = new VersionTagRequirement(options);

        var result = await requirement.MustAsync(null!);
        result.Success.ShouldBeFalse();
        result.Reason.ShouldNotBeNull();
        result.Reason.ShouldContain($"Invalid tag: '{refName}'");
    }

    [Test]
    [Arguments("v0.0.0")]
    [Arguments("v1.2.0")]
    [Arguments("v10.20.30")]
    public async Task ShouldPassIfTagIsValid(string refName)
    {
        var options = CreateOptions("tag", refName);

        var requirement = new VersionTagRequirement(options);

        var result = await requirement.MustAsync(null!);
        result.Success.ShouldBeTrue();
        result.Reason.ShouldBeNull();
    }

    private static IOptions<GitHubSettings> CreateOptions(string refType, string refName) =>
        Options.Create(
            new GitHubSettings
            {
                RefType = refType,
                RefName = refName,
                EventName = "push",
                OidcRequestUrl = "test",
                OidcRequestToken = "test",
            }
        );
}
