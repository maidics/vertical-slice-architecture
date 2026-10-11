using VsaTemplate.Features.Examples;

namespace VsaTemplate.UnitTests.Features.Examples.Update;

public sealed class UpdateExampleEndpointTests : EndpointTestBase<UpdateExampleEndpoint>
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
    //     UpdateExampleEndpoint.Map(spy);
    //
    //     var endpoints = spy.GetEndpoints();
    //     endpoints.Count.ShouldBe(1);
    //
    //     var metadata = endpoints[0].Metadata;
    //     metadata.ShouldHaveEndpointName("UpdateExample");
    //     metadata.ShouldHaveOneAuthMetadataWithRoles(Roles.User, Roles.Administrator);
    // }
}
