using VsaTemplate.Features.Examples;

namespace VsaTemplate.UnitTests.Features.Examples.GetAll;

public sealed class GetExamplesEndpointTests : EndpointUnitTestBase<GetExamplesEndpoint>
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
    //     GetExamplesEndpoint.Map(spy);
    //
    //     var endpoints = spy.GetEndpoints();
    //     endpoints.Count.ShouldBe(1);
    //
    //     var metadata = endpoints[0].Metadata;
    //     metadata.ShouldHaveEndpointName("GetExamples");
    //     metadata.ShouldNotHaveAuthMetadata();
    // }
}
