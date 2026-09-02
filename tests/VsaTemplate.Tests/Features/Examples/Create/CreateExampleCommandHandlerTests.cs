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
        var command = new CreateExampleCommand("test");

        var handler = GetRequiredService<CreateExampleCommandHandler>();

        var result = await handler.Handle(command, CancellationToken.None);
        result.ShouldBeSuccessful();

        await using var context = GetRequiredService<ApplicationDbContext>();

        var example = await context.Examples.FirstOrDefaultAsync(x => x.Id == result.Value);
        example.ShouldNotBeNull();
        example.Content.ShouldBe(command.Content);
    }
}
