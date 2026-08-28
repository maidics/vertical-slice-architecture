using VsaTemplate.Features.Examples;
using VsaTemplate.Features.Examples.GetAll;
using VsaTemplate.Infrastructure.Database;
using VsaTemplate.Tests.TestInfrastructure.FunctionalTests;

namespace VsaTemplate.Tests.Features.Examples.GetAll;

public sealed class GetExamplesTests : FunctionalTestBase
{
    [Test]
    [Arguments(0)]
    [Arguments(10)]
    public async Task ShouldReturnExamples(int exampleAmount)
    {
        List<Example> examples = [];

        await using var context = GetRequiredService<ApplicationDbContext>();

        for (var i = 0; i < exampleAmount; i++)
        {
            var example = new Example { Content = $"test{i}" };

            await context.Examples.AddAsync(example);
            await context.SaveChangesAsync();

            examples.Add(example);
        }

        var entityIds = examples.Select(x => x.Id).ToList();

        var handler = GetRequiredService<GetExamplesQueryHandler>();

        var dtos = await handler.Handle(CancellationToken.None);
        var dtoIds = dtos.Select(x => x.Id).ToList();

        entityIds.ShouldBeEquivalentTo(dtoIds);
    }
}
