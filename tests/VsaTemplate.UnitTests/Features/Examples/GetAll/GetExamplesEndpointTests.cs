using VsaTemplate.Features.Examples;

namespace VsaTemplate.UnitTests.Features.Examples.GetAll;

[InheritsTests]
public sealed class GetExamplesEndpointTests : EndpointUnitTestBase<GetExamplesEndpoint>
{
    protected override string ExpectedPrefix => "examples";
    protected override string[] ExpectedTags { get; } = [];

    [Test]
    public override async Task MapMethodShouldMapEndpointWithAttributes()
    {
        await using var spy = new EndpointRouteBuilderSpy();
        await spy.InitializeAsync();

        GetExamplesEndpoint.Map(spy);

        var endpoints = spy.GetEndpoints();
        endpoints.Count.ShouldBe(1);

        var metadata = endpoints[0].Metadata;
        metadata.ShouldHaveEndpointName("GetExamples");
        metadata.ShouldNotHaveAuthMetadata();
    }
}
