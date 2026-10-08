using Microsoft.EntityFrameworkCore;
using VsaTemplate.Common.Models;
using VsaTemplate.Domain.Entities;
using VsaTemplate.Features.Examples;
using VsaTemplate.Infrastructure.Database;
using VsaTemplate.Tests.TestInfrastructure;
using VsaTemplate.Tests.TestInfrastructure.FunctionalTests;

namespace VsaTemplate.Tests.Features.Examples.Create;

public sealed class CreateExampleCommandHandlerTests : FunctionalTestBase
{
    [Test]
    public async Task ShouldReturnConflictIfExampleWithContentExists()
    {
        var example = new Example { Content = "test" };

        await using var context = GetRequiredService<ApplicationDbContext>();
        await context.Examples.AddAsync(example);
        await context.SaveChangesAsync();

        var command = new CreateExampleCommand(example.Content);

        var handler = GetRequiredService<CreateExampleCommandHandler>();

        var result = await handler.Handle(command, CancellationToken.None);
        result.ShouldBeFailed(
            ResultType.Conflict,
            $"{nameof(Example)} already exists with content: {command.Content}"
        );
    }

    [Test]
    public async Task ShouldCreateExample()
    {
        var userId = Guid.NewGuid();
        GetRequiredService<FunctionalTestUser>().LogIn(userId, null);

        var timeProvider = GetRequiredService<TimeProvider>();

        var command = new CreateExampleCommand("test");

        var handler = GetRequiredService<CreateExampleCommandHandler>();

        var before = timeProvider.GetUtcNow();
        var result = await handler.Handle(command, CancellationToken.None);
        var after = timeProvider.GetUtcNow();
        result.ShouldBeSuccessful();

        await using var context = GetRequiredService<ApplicationDbContext>();

        var example = await context.Examples.FirstOrDefaultAsync(x => x.Id == result.Value);
        example.ShouldNotBeNull();
        example.Content.ShouldBe(command.Content);
        example.CreatedBy.ShouldBe(userId);
        example.CreatedOn.ShouldBeInRange(before, after);
        example.LastModifiedBy.ShouldBe(userId);
        example.LastModifiedOn.ShouldBe(example.CreatedOn);
    }
}
