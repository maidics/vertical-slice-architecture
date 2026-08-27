using VsaTemplate.Common.Models;
using VsaTemplate.Features.Examples;
using VsaTemplate.Features.Examples.Queries.GetExampleById;
using VsaTemplate.Infrastructure.Database;
using VsaTemplate.Tests.TestInfrastructure;
using VsaTemplate.Tests.TestInfrastructure.FunctionalTests;

namespace VsaTemplate.Tests.Features.Examples.Queries;

public sealed class GetExampleByIdTests : FunctionalTestBase
{
    [Test]
    public async Task ShouldReturnNotFoundIfExampleDoesNotExists()
    {
        var query = new GetExampleByIdQuery(Guid.Empty);
        var handler = GetRequiredService<GetExampleByIdQueryHandler>();

        var result = await handler.Handle(query, CancellationToken.None);

        result.ShouldBeFailed(ResultType.NotFound, ["Example not found."]);
    }

    [Test]
    public async Task ShouldReturnSuccessIfExampleExists()
    {
        var example = new Example { Content = "test" };

        await using var context = GetRequiredService<ApplicationDbContext>();
        await context.AddAsync(example);
        await context.SaveChangesAsync();

        var handler = GetRequiredService<GetExampleByIdQueryHandler>();

        var query = new GetExampleByIdQuery(example.Id);
        var result = await handler.Handle(query, CancellationToken.None);

        result.ShouldBeSuccessful();
        result.Value.ShouldBeEquivalentTo(new ExampleDto(example.Id, example.Content, false));
    }
}
