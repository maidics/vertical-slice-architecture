using VsaTemplate.Domain.Constants;
using VsaTemplate.Features.Examples;

namespace VsaTemplate.UnitTests.Features.Examples.Delete;

[InheritsTests]
public sealed class DeleteExampleEndpointTests : EndpointUnitTestBase<DeleteExampleEndpoint>
{
    protected override string ExpectedPrefix => "examples";
    protected override string[] ExpectedTags { get; } = [];

    [Test]
    public override async Task MapMethodShouldMapEndpointWithAttributes()
    {
        await using var spy = new EndpointRouteBuilderSpy();
        await spy.InitializeAsync();

        DeleteExampleEndpoint.Map(spy);

        var endpoints = spy.GetEndpoints();
        endpoints.Count.ShouldBe(1);

        var metadata = endpoints[0].Metadata;
        metadata.ShouldHaveEndpointName("DeleteExample");
        metadata.ShouldHaveOneAuthMetadataWithRoles(Roles.Administrator);
    }
}
