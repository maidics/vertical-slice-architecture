using Microsoft.EntityFrameworkCore;
using VsaTemplate.Common.Models;
using VsaTemplate.Features.Examples;
using VsaTemplate.Features.Examples.AppendContent;
using VsaTemplate.Infrastructure.Database;
using VsaTemplate.Tests.TestInfrastructure;
using VsaTemplate.Tests.TestInfrastructure.FunctionalTests;

namespace VsaTemplate.Tests.Features.Examples.Commands.AppendContent;

public sealed class AppendExampleContentHandlerTests : FunctionalTestBase
{
    [Test]
    public async Task ShouldReturnNotFoundIfExampleDoesNotExists()
    {
        var command = new AppendExampleContentCommand(Guid.Empty, "test");

        var handler = GetRequiredService<AppendExampleContentCommandHandler>();

        var result = await handler.Handle(command, CancellationToken.None);
        result.ShouldBeFailed(ResultType.NotFound, "Example not found.");
    }

    [Test]
    public async Task ShouldReturnConflictIfExampleExistsWithContent()
    {
        var example1 = new Example { Content = "test-content" };
        var example2 = new Example { Content = "test" };

        await using var context = GetRequiredService<ApplicationDbContext>();
        await context.Examples.AddRangeAsync(example1, example2);
        await context.SaveChangesAsync();

        var command = new AppendExampleContentCommand(example2.Id, "-content");
        var handler = GetRequiredService<AppendExampleContentCommandHandler>();

        var result = await handler.Handle(command, CancellationToken.None);
        result.ShouldBeFailed(
            ResultType.Conflict,
            $"Example with '{example1.Content}' content already exists."
        );
    }

    [Test]
    public async Task ShouldAppendContent()
    {
        var example = new Example { Content = "test" };

        await using var context = GetRequiredService<ApplicationDbContext>();
        await context.AddAsync(example);
        await context.SaveChangesAsync();

        var command = new AppendExampleContentCommand(example.Id, "-content");

        var handler = GetRequiredService<AppendExampleContentCommandHandler>();

        var result = await handler.Handle(command, CancellationToken.None);
        result.ShouldBeSuccessful();

        var updated = await context.Examples.FirstOrDefaultAsync(e => e.Id == example.Id);
        updated.ShouldNotBeNull();
        updated.Content.ShouldBe("test-content");
    }
}
