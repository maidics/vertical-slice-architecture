using VsaTemplate.Common.Models;
using VsaTemplate.Features.Examples;
using VsaTemplate.Features.Examples.GetById;
using VsaTemplate.Infrastructure.Database;
using VsaTemplate.Tests.TestInfrastructure;
using VsaTemplate.Tests.TestInfrastructure.FunctionalTests;

namespace VsaTemplate.Tests.Features.Examples.GetById;

public sealed class GetExampleByIdTests : FunctionalTestBase
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
        result.Value.ShouldBeEquivalentTo(new ExampleDto(example.Id, example.Content, false));
    }
}
