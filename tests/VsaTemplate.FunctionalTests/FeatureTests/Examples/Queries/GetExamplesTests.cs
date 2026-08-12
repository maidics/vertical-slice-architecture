using Shouldly;
using VsaTemplate.Features.Examples;
using VsaTemplate.Features.Examples.Queries;
using VsaTemplate.FunctionalTests.Infrastructure;
using VsaTemplate.FunctionalTests.Infrastructure.Common;

namespace VsaTemplate.FunctionalTests.FeatureTests.Examples.Queries;

public sealed class GetExamplesTests : ApplicationTestBase
{
    [TestCase(0)]
    [TestCase(10)]
    public async Task ShouldReturnExamples(int exampleAmount)
    {
        List<Example> examples = [];

        for (var i = 0; i < exampleAmount; i++)
        {
            var example = new Example { Content = $"test{i}" };

            await Testing.AddAsync(example);

            examples.Add(example);
        }

        var entityIds = examples.Select(x => x.Id).ToList();

        var handler = GetService<GetExamplesQueryHandler>();

        var dtos = await handler.Handle(CancellationToken.None);
        var dtoIds = dtos.Select(x => x.Id).ToList();

        entityIds.ShouldBeEquivalentTo(dtoIds);
    }
}
