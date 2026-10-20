using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Octokit;
using Shouldly;
using VsaTemplate.Pipeline.Constants;
using VsaTemplate.Pipeline.Modules;
using VsaTemplate.Pipeline.Tests.TestInfrastructure.BaseClasses;
using VsaTemplate.Pipeline.Tests.TestInfrastructure.Extensions;

namespace VsaTemplate.Pipeline.Tests.Modules;

[InheritsTests]
public sealed class DraftReleaseModuleTests()
    : ModuleTestBase<DraftReleaseModule>(typeof(NuGetPushModule))
{
    private const string _tag = "v2.0.0";
    private const string _owner = Template.Repository.Owner;
    private const string _repo = Template.Repository.Name;
    private readonly FakeLogger _logger = new();
    private readonly IReleasesClientMock _client = IReleasesClient.Mock();

    [Test]
    public async Task CreateReleaseIfNotExistsShouldThrowIfMoreThanOneReleaseExistsForTag()
    {
        IReadOnlyList<Release> releases =
        [
            Release.Create(tagName: _tag),
            Release.Create(tagName: _tag),
        ];

        _client.GetAll(_owner, _repo).ReturnsAsync(Task.FromResult(releases));

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            DraftReleaseModule.CreateReleaseIfNotExists(
                _client.Object,
                _owner,
                _repo,
                _tag,
                _logger
            )
        );
        ex.Message.ShouldBe("Sequence contains more than one matching element");

        _logger.Collector.Count.ShouldBe(0);

        _client.Create(Any(), Any(), Any()).WasNeverCalled();
    }

    [Test]
    public async Task CreateReleaseIfNotExistsShouldThrowIfNonDraftReleaseExists()
    {
        IReadOnlyList<Release> releases = [Release.Create(draft: false, tagName: _tag)];

        _client.GetAll(_owner, _repo).ReturnsAsync(Task.FromResult(releases));

        var ex = await Should.ThrowAsync<InvalidOperationException>(() =>
            DraftReleaseModule.CreateReleaseIfNotExists(_client, _owner, _repo, _tag, _logger)
        );
        ex.Message.ShouldBe($"Release for '{_tag}' tag is already published.");

        _client.GetAll(_owner, _repo).WasCalled(Times.Once);

        _logger.Collector.Count.ShouldBe(0);

        _client.Create(Any(), Any(), Any()).WasNeverCalled();
    }

    [Test]
    public async Task CreateReleaseIfNotExistsReturnEarlyIfExistingIsDraft()
    {
        IReadOnlyList<Release> releases = [Release.Create(draft: true, tagName: _tag, id: 999)];

        _client.GetAll(_owner, _repo).ReturnsAsync(Task.FromResult(releases));

        await DraftReleaseModule.CreateReleaseIfNotExists(_client, _owner, _repo, _tag, _logger);

        _client.GetAll(_owner, _repo).WasCalled(Times.Once);

        _logger.Collector.Count.ShouldBe(1);
        _logger.Collector.LatestRecord.Level.ShouldBe(LogLevel.Information);
        _logger.Collector.LatestRecord.Message.ShouldBe(
            $"Found existing draft release: {_tag} ({releases[0].Id})."
        );

        _client.Create(Any(), Any(), Any()).WasNeverCalled();
    }

    [Test]
    public async Task CreateReleaseIfNotExistsShouldCreateDraftReleaseIfNotExists()
    {
        IReadOnlyList<Release> releases = [];

        _client.GetAll(_owner, _repo).ReturnsAsync(Task.FromResult(releases));

        await DraftReleaseModule.CreateReleaseIfNotExists(_client, _owner, _repo, _tag, _logger);

        _client.GetAll(_owner, _repo).WasCalled(Times.Once);

        _logger.Collector.Count.ShouldBe(1);
        _logger.Collector.LatestRecord.Level.ShouldBe(LogLevel.Information);
        _logger.Collector.LatestRecord.Message.ShouldBe($"Creating new release for {_tag} tag.");

        _client
            .Create(_owner, _repo, r => r is { Draft: true, GenerateReleaseNotes: true })
            .WasCalled(Times.Once);
    }
}
