using VsaTemplate.Domain.Constants;
using VsaTemplate.Features.Examples;

namespace VsaTemplate.UnitTests.Features.Examples.Update;

[InheritsTests]
public sealed class UpdateExampleEndpointTests : EndpointUnitTestBase<UpdateExampleEndpoint>
{
    protected override string ExpectedPrefix => "examples";
    protected override string[] ExpectedTags { get; } = [];

    [Test]
    public override async Task MapMethodShouldMapEndpointWithAttributes()
    {
        await using var spy = new EndpointRouteBuilderSpy();
        await spy.InitializeAsync();

        UpdateExampleEndpoint.Map(spy);

        var endpoints = spy.GetEndpoints();
        endpoints.Count.ShouldBe(1);

        var metadata = endpoints[0].Metadata;
        metadata.ShouldHaveEndpointName("UpdateExample");
        metadata.ShouldHaveOneAuthMetadataWithRoles(Roles.User, Roles.Administrator);
    }
}
