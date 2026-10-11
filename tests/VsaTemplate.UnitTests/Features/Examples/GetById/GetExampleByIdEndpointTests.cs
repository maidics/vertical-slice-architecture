using VsaTemplate.Features.Examples;

namespace VsaTemplate.UnitTests.Features.Examples.GetById;

[InheritsTests]
public sealed class GetExampleByIdEndpointTests : EndpointUnitTestBase<GetExampleByIdEndpoint>
{
    protected override string ExpectedPrefix => "examples";
    protected override string[] ExpectedTags { get; } = [];

    [Test]
    public override async Task MapMethodShouldMapEndpointWithAttributes()
    {
        await using var spy = new EndpointRouteBuilderSpy();
        await spy.InitializeAsync();

        GetExampleByIdEndpoint.Map(spy);

        var endpoints = spy.GetEndpoints();
        endpoints.Count.ShouldBe(1);

        var metadata = endpoints[0].Metadata;
        metadata.ShouldHaveEndpointName("GetExampleById");
        metadata.ShouldHaveOneAuthMetadataWithoutRoles();
    }
}
