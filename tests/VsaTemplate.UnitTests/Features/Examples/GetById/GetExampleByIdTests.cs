using VsaTemplate.Common.Models;
using VsaTemplate.Domain.Entities;
using VsaTemplate.Features.Examples;
using VsaTemplate.Infrastructure.Database;

namespace VsaTemplate.UnitTests.Features.Examples.GetById;

public sealed class GetExampleByIdTests : UnitTestBase
{
    [Test]
    public async Task ShouldReturnNotFoundIfExampleDoesNotExists()
    {
        var handler = GetRequiredService<GetExampleByIdQueryHandler>();

        var result = await handler.Handle(Guid.Empty, CancellationToken.None);

        result.ShouldBeFailed(ResultType.NotFound, $"{nameof(Example)} not found.");
    }

    [Test]
    public async Task ShouldReturnSuccessIfExampleExists()
    {
        var example = new Example { Content = "test" };

        await using var context = GetRequiredService<ApplicationDbContext>();
        await context.AddAsync(example);
        await context.SaveChangesAsync();

        var handler = GetRequiredService<GetExampleByIdQueryHandler>();

        var result = await handler.Handle(example.Id, CancellationToken.None);

        result.ShouldBeSuccessful();
        result.Value.ShouldBeEquivalentTo(new ExampleDto(example.Id, example.Content));
    }
}
