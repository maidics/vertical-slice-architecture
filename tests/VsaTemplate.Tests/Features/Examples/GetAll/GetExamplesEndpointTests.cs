using System.Net.Http.Json;
using VsaTemplate.Features.Examples;
using VsaTemplate.Features.Examples.GetAll;
using VsaTemplate.Tests.TestInfrastructure.WebTests;

namespace VsaTemplate.Tests.Features.Examples.GetAll;

public sealed class GetExamplesEndpointTests : EndpointTestBase<GetExamplesEndpoint>
{
    protected override string Endpoint => "api/examples";

    [Test]
    public override void ShouldHaveCorrectPrefix()
    {
        Prefix.ShouldBe("examples");
    }

    [Test]
    public override void ShouldHaveCorrectTags()
    {
        Tags.ShouldBeEquivalentTo(Array.Empty<string>());
    }

    [Test]
    [Arguments(0)]
    [Arguments(2)]
    [Arguments(20)]
    public async Task ShouldReturnAllExamples(int exampleCount)
    {
        List<Example> examples = [];

        await using var context = CreateDbContext();

        for (int i = 0; i < exampleCount; i++)
        {
            var example = new Example { Content = $"test{i}" };

            await context.Examples.AddAsync(example);
            await context.SaveChangesAsync();

            examples.Add(example);
        }

        using var client = CreateHttpClient();

        var response = await client.GetFromJsonAsync<List<ExampleDto>>(Endpoint);
        response.ShouldNotBeNull();

        var dtoIds = response.Select(e => e.Id).ToList();
        dtoIds.ShouldBeEquivalentTo(examples.Select(e => e.Id).ToList());
    }
}
