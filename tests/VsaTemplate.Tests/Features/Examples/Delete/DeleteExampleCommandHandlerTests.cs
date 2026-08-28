using Microsoft.EntityFrameworkCore;
using VsaTemplate.Common.Models;
using VsaTemplate.Features.Examples;
using VsaTemplate.Features.Examples.Delete;
using VsaTemplate.Infrastructure.Database;
using VsaTemplate.Tests.TestInfrastructure;
using VsaTemplate.Tests.TestInfrastructure.FunctionalTests;

namespace VsaTemplate.Tests.Features.Examples.Delete;

public sealed class DeleteExampleCommandHandlerTests : FunctionalTestBase
{
    [Test]
    public async Task ShouldReturnNotFoundIfExampleDoesNotExists()
    {
        var command = new DeleteExampleCommand(Guid.Empty);
        var handler = GetRequiredService<DeleteExampleCommandHandler>();

        var result = await handler.Handle(command, CancellationToken.None);
        result.ShouldBeFailed(ResultType.NotFound, "Example not found.");
    }

    [Test]
    public async Task ShouldDeleteExample()
    {
        var example = new Example { Content = "test" };

        await using var context = GetRequiredService<ApplicationDbContext>();
        await context.Examples.AddAsync(example);
        await context.SaveChangesAsync();

        var command = new DeleteExampleCommand(example.Id);
        var handler = GetRequiredService<DeleteExampleCommandHandler>();

        var result = await handler.Handle(command, CancellationToken.None);
        result.ShouldBeSuccessful();

        var deleted = await context.Examples.FirstOrDefaultAsync(x => x.Id == example.Id);
        deleted.ShouldBeNull();
    }
}
