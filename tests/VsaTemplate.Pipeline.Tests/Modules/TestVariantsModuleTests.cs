using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using ModularPipelines.DotNet.Services;
using Shouldly;
using VsaTemplate.Pipeline.Constants;
using VsaTemplate.Pipeline.Interfaces;
using VsaTemplate.Pipeline.Models;
using VsaTemplate.Pipeline.Modules;
using VsaTemplate.Pipeline.Settings;
using VsaTemplate.Pipeline.Tests.TestInfrastructure.BaseClasses;
using VsaTemplate.Pipeline.Tests.TestInfrastructure.Extensions;
using VsaTemplate.Pipeline.Tests.TestInfrastructure.Services;

namespace VsaTemplate.Pipeline.Tests.Modules;

[InheritsTests]
public sealed class TestVariantsModuleTests()
    : ModuleTestBase<TestVariantsModule>(typeof(BuildVariantsModule))
{
    [Test]
    [Arguments("pull_request", "branch", "42/merge")]
    [Arguments("push", "branch", "main")]
    [Arguments("push", "branch", "feat/pgsql-db-option")]
    [Arguments("push", "tag", "v1.0.0")]
    public async Task ShouldSkipOnNonPrAndNonMainPush(
        string eventName,
        string refType,
        string refName
    )
    {
        var options = Options.Create(
            new GitHubSettings
            {
                RefType = refType,
                RefName = refName,
                EventName = eventName,
                OidcRequestUrl = "test",
                OidcRequestToken = "test",
            }
        );

        var module = new TestVariantsModule(
            options,
            ISolutionFilterBuilder.Mock().Object,
            IMemoryInfo.Mock().Object
        );
        var config = module.GetConfiguration();
        await config.ShouldSkipOnNonPrAndNonMainPushAsync(
            options.Value.IsPullRequest || options.Value.IsMainPush
        );
    }

    [Test]
    public async Task CreateSolutionFiltersShouldCreateFilterPerVariant()
    {
        const string filter = nameof(filter);
        var builder = ISolutionFilterBuilder.Mock();
        builder.WithName(Any()).Returns(builder.Object);
        builder.WithSolution(Any()).Returns(builder.Object);
        builder.WithProjects(Any()).Returns(builder.Object);
        builder.BuildAsync().Returns(filter);

        var variants = TemplateVariant.All;

        var filters = await TestVariantsModule.CreateSolutionFilters(builder, variants);

        filters.Count.ShouldBe(variants.Count);
        filters.ShouldAllBe(f => f == filter);

        foreach (var variant in variants)
        {
            builder.WithName($"Variant.{variant.Name}").WasCalled(Times.Once);
            builder.WithSolution(variant.Solution).WasCalled(Times.Once);
            builder
                .WithProjects(p => p.SequenceEqual(variant.TestProjects))
                .WasCalled(Times.Once);
        }

        builder.BuildAsync().WasCalled(Times.Exactly(variants.Count));
    }

    [Test]
    public async Task BuildAsyncProcessorShouldBuildProcessor()
    {
        var dotnet = IDotNet.Mock();
        var memoryInfo = IMemoryInfo.Mock();
        ILogger logger = new FakeLogger();

        await using var fileSystem = new TestFileSystem();
        string[] filters =
        [
            fileSystem.CreateTestFile("Variant.A.slnf"),
            fileSystem.CreateTestFile("Variant.B.slnf"),
        ];

        await using var processor = TestVariantsModule.BuildAsyncProcessor(
            filters,
            dotnet,
            memoryInfo.Object,
            logger,
            CancellationToken.None
        );

        var tasks = processor.GetEnumerableTasks().ToList();
        tasks.Count.ShouldBe(filters.Length);
        processor.GetMaxConcurrency().ShouldBe(int.MaxValue);

        await processor.GetResultsAsync();

        foreach (var filter in filters)
        {
            dotnet
                .Test(
                    o =>
                        o.Solution == filter
                        && o.NoRestore == true
                        && o.NoBuild == true
                        && o.Configuration == Template.Configuration,
                    Any(),
                    Any()
                )
                .WasCalled(Times.Once);
        }

        memoryInfo
            .LogWarningIfUsageIsGreaterThan(l => ReferenceEquals(l, logger), 80)
            .WasCalled(Times.Exactly(filters.Length));
    }

    [Test]
    public async Task BuildAsyncProcessorShouldThrowIfSolutionFilterNotFound()
    {
        await using var fileSystem = new TestFileSystem();
        var existing = fileSystem.CreateTestFile("Variant.A.slnf");
        var missing = Path.Combine(fileSystem.TempDir, "Variant.B.slnf");

        var ex = Should.Throw<FileNotFoundException>(() =>
            TestVariantsModule.BuildAsyncProcessor(
                [existing, missing],
                IDotNet.Mock().Object,
                IMemoryInfo.Mock().Object,
                new FakeLogger(),
                CancellationToken.None
            )
        );
        ex.Message.ShouldBe($"Solution filter(s) not found: {missing}.");
    }
}
