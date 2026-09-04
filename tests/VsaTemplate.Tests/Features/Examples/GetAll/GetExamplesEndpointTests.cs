using System.Net.Http.Json;
using VsaTemplate.Domain.Entities;
using VsaTemplate.Features.Examples;
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
    public override void MapMethodShouldMapEndpointWithAttributes()
    {
        var spy = CreateEndpointRouteBuilderSpy();

        GetExamplesEndpoint.Map(spy);

        var endpoints = spy.GetEndpoints();
        endpoints.Count.ShouldBe(1);

        var metadata = endpoints[0].Metadata;
        metadata.ShouldHaveEndpointName("GetExamples");
        metadata.ShouldNotHaveAuthMetadata();
    }

    [Test]
    [Arguments(0)]
    [Arguments(2)]
    [Arguments(20)]
    public async Task ShouldReturnAllExamples(int exampleCount)
    {
        var examples = Enumerable
            .Range(0, exampleCount)
            .Select(i => new Example { Content = $"test{i}" })
            .ToList();

        await SeedAsync([.. examples]);

        using var client = CreateHttpClient();

        var response = await client.GetFromJsonAsync<List<ExampleDto>>(Endpoint);
        response.ShouldNotBeNull();

        var dtoIds = response.Select(e => e.Id).Order().ToList();
        dtoIds.ShouldBeEquivalentTo(examples.Select(e => e.Id).Order().ToList());
    }
}
