using Shouldly;
using VsaTemplate.Common.Models;
using VsaTemplate.Features.Examples;
using VsaTemplate.Features.Examples.Commands;
using VsaTemplate.FunctionalTests.Infrastructure;
using VsaTemplate.FunctionalTests.Infrastructure.Common;

namespace VsaTemplate.FunctionalTests.FeatureTests.Examples.Commands;

public sealed class UpdateExampleTests : ApplicationTestBase
{
    [Test]
    public async Task ShouldReturnNotFoundIfExampleDoesNotExists()
    {
        var command = new UpdateExampleCommand(Guid.Empty, "test");

        var handler = GetService<UpdateExampleCommandHandler>();

        var result = await handler.Handle(command, CancellationToken.None);
        result.ShouldBeFailed(ResultType.NotFound, ["Example not found."]);
    }

    [Test]
    public async Task ShouldUpdateExampleContent()
    {
        var example = new Example { Content = "test" };

        await Testing.AddAsync(example);

        var command = new UpdateExampleCommand(example.Id, "new-test-content");

        var handler = GetService<UpdateExampleCommandHandler>();

        var result = await handler.Handle(command, CancellationToken.None);
        result.ShouldBeSuccessful();

        var updated = await Testing.FirstOrDefaultAsync<Example>(x => x.Id == example.Id);
        updated!.Content.ShouldBe(command.Content);
    }
}
