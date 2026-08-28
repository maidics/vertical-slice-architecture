using Microsoft.EntityFrameworkCore;
using VsaTemplate.Common.Models;
using VsaTemplate.Features.Examples;
using VsaTemplate.Features.Examples.Update;
using VsaTemplate.Infrastructure.Database;
using VsaTemplate.Tests.TestInfrastructure;
using VsaTemplate.Tests.TestInfrastructure.FunctionalTests;

namespace VsaTemplate.Tests.Features.Examples.Update;

public sealed class UpdateExampleCommandHandlerTests : FunctionalTestBase
{
    [Test]
    public async Task ShouldReturnNotFoundIfExampleDoesNotExists()
    {
        var command = new UpdateExampleCommand(Guid.Empty, "test");

        var handler = GetRequiredService<UpdateExampleCommandHandler>();

        var result = await handler.Handle(command, CancellationToken.None);
        result.ShouldBeFailed(ResultType.NotFound, ["Example not found."]);
    }

    [Test]
    public async Task ShouldReturnConflictIfExampleWithContentAlreadyExists()
    {
        var example1 = new Example { Content = "test" };
        var example2 = new Example { Content = "" };

        await using var context = GetRequiredService<ApplicationDbContext>();
        await context.AddRangeAsync(example1, example2);
        await context.SaveChangesAsync();

        var command = new UpdateExampleCommand(example2.Id, example1.Content);
        var handler = GetRequiredService<UpdateExampleCommandHandler>();

        var result = await handler.Handle(command, CancellationToken.None);
        result.ShouldBeFailed(
            ResultType.Conflict,
            $"Example with '{example1.Content}' content already exists."
        );
    }

    [Test]
    public async Task ShouldUpdateExampleContent()
    {
        var example = new Example { Content = "test" };

        await using var context = GetRequiredService<ApplicationDbContext>();
        await context.AddAsync(example);
        await context.SaveChangesAsync();

        var command = new UpdateExampleCommand(example.Id, "new-test-content");

        var handler = GetRequiredService<UpdateExampleCommandHandler>();

        var result = await handler.Handle(command, CancellationToken.None);
        result.ShouldBeSuccessful();

        var updated = await context.Examples.FirstOrDefaultAsync(x => x.Id == example.Id);
        updated.ShouldNotBeNull();
        updated.Content.ShouldBe(command.Content);
    }

    [Test]
    public async Task ShouldReturnSuccessIfNewContentIsTheSameAsCurrent()
    {
        var example = new Example { Content = "test" };

        await using var context = GetRequiredService<ApplicationDbContext>();
        await context.AddAsync(example);
        await context.SaveChangesAsync();

        var command = new UpdateExampleCommand(example.Id, example.Content);

        var handler = GetRequiredService<UpdateExampleCommandHandler>();

        var result = await handler.Handle(command, CancellationToken.None);
        result.ShouldBeSuccessful();
    }
}
