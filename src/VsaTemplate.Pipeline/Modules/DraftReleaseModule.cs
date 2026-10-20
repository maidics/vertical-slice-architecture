using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModularPipelines.Attributes;
using ModularPipelines.Configuration;
using ModularPipelines.Context;
using ModularPipelines.GitHub.Extensions;
using ModularPipelines.Modules;
using Octokit;
using VsaTemplate.Pipeline.Constants;
using VsaTemplate.Pipeline.Settings;

namespace VsaTemplate.Pipeline.Modules;

[DependsOn<NuGetPushModule>]
public sealed class DraftReleaseModule : Module
{
    private readonly GitHubSettings _settings;

    public DraftReleaseModule(IOptions<GitHubSettings> options)
    {
        _settings = options.Value;
    }

    protected override ModuleConfiguration Configure()
    {
        return ModuleConfiguration.Create().WithSkipWhen(SkipDecisions.TagPush(_settings)).Build();
    }

    protected override async Task ExecuteModuleAsync(
        IModuleContext context,
        CancellationToken cancellationToken
    )
    {
        await CreateReleaseIfNotExists(
            context.GitHub().Client.Repository.Release,
            Template.Repository.Owner,
            Template.Repository.Name,
            _settings.RefName,
            context.Logger
        );
    }

    public static async Task CreateReleaseIfNotExists(
        IReleasesClient releasesClient,
        string owner,
        string repo,
        string tag,
        ILogger logger
    )
    {
        var existing = (await releasesClient.GetAll(owner, repo)).SingleOrDefault(r =>
            r.TagName == tag
        );

        if (existing is { Draft: false })
            throw new InvalidOperationException($"Release for '{tag}' tag is already published.");

        if (existing is not null)
        {
            logger.LogInformation(
                "Found existing draft release: {Tag} ({ReleaseId}).",
                tag,
                existing.Id
            );
            return;
        }

        logger.LogInformation("Creating new release for {Tag} tag.", tag);

        await releasesClient.Create(
            owner,
            repo,
            new NewRelease(tag) { Draft = true, GenerateReleaseNotes = true }
        );
    }
}
