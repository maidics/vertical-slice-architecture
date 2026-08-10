using Shouldly;
using VsaTemplate.Common.Models;
using VsaTemplate.Features.Examples;
using VsaTemplate.Features.Examples.Commands;
using VsaTemplate.FunctionalTests.Infrastructure;
using VsaTemplate.FunctionalTests.Infrastructure.Common;

namespace VsaTemplate.FunctionalTests.FeatureTests.Examples.Commands;

public sealed class DeleteExampleTests : ApplicationTestBase
{
    [Test]
    public async Task ShouldReturnNotFoundIfExampleDoesNotExists()
    {
        var command = new DeleteExampleCommand(Guid.Empty);
        var handler = GetService<DeleteExampleCommandHandler>();

        var result = await handler.Handle(command, CancellationToken.None);
        result.ShouldBeFailed(ResultType.NotFound, ["Example not found."]);
    }

    [Test]
    public async Task ShouldDeleteExample()
    {
        var example = new Example { Content = "test" };

        await Testing.AddAsync(example);

        var command = new DeleteExampleCommand(example.Id);
        var handler = GetService<DeleteExampleCommandHandler>();

        var result = await handler.Handle(command, CancellationToken.None);
        result.ShouldBeSuccessful();

        var deleted = await Testing.FirstOrDefaultAsync<Example>(x => x.Id == example.Id);
        deleted.ShouldBeNull();
    }
}
