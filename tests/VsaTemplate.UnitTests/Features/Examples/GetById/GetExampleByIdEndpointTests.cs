using VsaTemplate.Features.Examples;

namespace VsaTemplate.UnitTests.Features.Examples.GetById;

public sealed class GetExampleByIdEndpointTests : EndpointUnitTestBase<GetExampleByIdEndpoint>
{
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

    // [Test]
    // public override async Task MapMethodShouldMapEndpointWithAttributes()
    // {
    //     await using var spy = new EndpointRouteBuilderSpy();
    //     await spy.InitializeAsync();
    //
    //     GetExampleByIdEndpoint.Map(spy);
    //
    //     var endpoints = spy.GetEndpoints();
    //     endpoints.Count.ShouldBe(1);
    //
    //     var metadata = endpoints[0].Metadata;
    //     metadata.ShouldHaveEndpointName("GetExampleById");
    //     metadata.ShouldHaveOneAuthMetadataWithoutRoles();
    // }
}
