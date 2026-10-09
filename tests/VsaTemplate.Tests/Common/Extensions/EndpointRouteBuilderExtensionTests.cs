using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using VsaTemplate.Common.Extensions;
using VsaTemplate.Tests.TestInfrastructure.TemplateTests;
using VsaTemplate.Tests.TestInfrastructure.WebTests;

namespace VsaTemplate.Tests.Common.Extensions;

public sealed class EndpointRouteBuilderExtensionTests
{
    [ClassDataSource<EndpointRouteBuilderSpy>]
    public required EndpointRouteBuilderSpy Spy { get; init; }

    [Test]
    public void MapMethodsShouldThrowIsDelegateIsAnonymous()
    {
        Should.Throw<ArgumentException>(() => Spy.MapGet(() => { }));
        Should.Throw<ArgumentException>(() => Spy.MapPost(() => { }));
        Should.Throw<ArgumentException>(() => Spy.MapPut(() => { }, "test"));
        Should.Throw<ArgumentException>(() => Spy.MapPatch(() => { }, "test"));
        Should.Throw<ArgumentException>(() => Spy.MapDelete(() => { }, "test"));
    }

    private void TestEndpointMethod() { }

    [Test]
    public void MapMethodsShouldNotThrowIfDelegateIsNotAnonymous()
    {
        Should.NotThrow(() => Spy.MapGet(TestEndpointMethod));
        Should.NotThrow(() => Spy.MapPost(TestEndpointMethod));
        Should.NotThrow(() => Spy.MapPut(TestEndpointMethod, "test"));
        Should.NotThrow(() => Spy.MapPatch(TestEndpointMethod, "test"));
        Should.NotThrow(() => Spy.MapDelete(TestEndpointMethod, "test"));
    }

    [Test]
    public void MapEndpointsShouldMapAllEndpointsFromAssembly()
    {
        Spy.MapEndpoints(typeof(EndpointRouteBuilderExtensionTests).Assembly);

        var endpoints = Spy.GetEndpoints();
        endpoints.Count.ShouldBe(2);

        var names = endpoints
            .Select(e => e.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName)
            .ToList();
        names.Count.ShouldBe(2);

        names.ShouldContain(nameof(TestGetEndpoint.Get));
        names.ShouldContain(nameof(TestPostEndpoint.Post));

        foreach (var endpoint in endpoints)
        {
            var tags = endpoint.Metadata.GetMetadata<ITagsMetadata>()!.Tags.ToArray();

            tags.Length.ShouldBe(1);
            tags.ShouldBeEquivalentTo(new[] { "Test" });

            endpoint.DisplayName!.ShouldContain("Test");
        }
    }

    [Test]
    public void MapLogoutEndpointShouldMapLogout()
    {
        Spy.MapLogoutEndpoint();

        var endpoints = Spy.GetEndpoints();
        endpoints.Count.ShouldBe(1);

        var endpoint = endpoints.First();
        endpoint.DisplayName.ShouldNotBeNull();
        endpoint.DisplayName.ShouldContain("post");
        endpoint.DisplayName.ShouldContain("/identity/logout");

        var tagsMetadata = endpoint.Metadata.GetMetadata<ITagsMetadata>();
        tagsMetadata.ShouldNotBeNull();
        tagsMetadata.Tags.Count.ShouldBe(1);
        tagsMetadata.Tags[0].ShouldBe("Users");
    }
}
