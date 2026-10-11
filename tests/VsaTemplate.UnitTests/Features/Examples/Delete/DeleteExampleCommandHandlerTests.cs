using Microsoft.EntityFrameworkCore;
using VsaTemplate.Common.Models;
using VsaTemplate.Domain.Entities;
using VsaTemplate.Features.Examples;
using VsaTemplate.Infrastructure.Database;

namespace VsaTemplate.UnitTests.Features.Examples.Delete;

public sealed class DeleteExampleCommandHandlerTests : UnitTestBase
{
    [Test]
    public async Task ShouldReturnNotFoundIfExampleDoesNotExists()
    {
        var handler = GetRequiredService<DeleteExampleCommandHandler>();

        var result = await handler.Handle(Guid.Empty, CancellationToken.None);
        result.ShouldBeFailed(ResultType.NotFound, $"{nameof(Example)} not found.");
    }

    [Test]
    public async Task ShouldDeleteExample()
    {
        var example = new Example { Content = "test" };

        await using var context = GetRequiredService<ApplicationDbContext>();
        await context.Examples.AddAsync(example);
        await context.SaveChangesAsync();

        var handler = GetRequiredService<DeleteExampleCommandHandler>();

        var result = await handler.Handle(example.Id, CancellationToken.None);
        result.ShouldBeSuccessful();

        var deleted = await context.Examples.FirstOrDefaultAsync(x => x.Id == example.Id);
        deleted.ShouldBeNull();
    }
}
