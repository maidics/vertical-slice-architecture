using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Shouldly;
using VsaTemplate.Common.Extensions;
using VsaTemplate.TemplateTests.Infrastructure;
using VsaTemplate.TemplateTests.Infrastructure.Common;
using VsaTemplate.TemplateTests.Infrastructure.Common.BaseClasses;

namespace VsaTemplate.TemplateTests.Tests;

public sealed class EndpointRouteBuilderExtensionTests : TestBase
{
    [Test]
    public void MapMethodsShouldThrowIsDelegateIsAnonymous()
    {
        var spy = GetRequiredService<EndpointRouteBuilderSpy>();

        Should.Throw<ArgumentException>(() => spy.MapGet(() => { }));
        Should.Throw<ArgumentException>(() => spy.MapPost(() => { }));
        Should.Throw<ArgumentException>(() => spy.MapPut(() => { }, "test"));
        Should.Throw<ArgumentException>(() => spy.MapPatch(() => { }, "test"));
        Should.Throw<ArgumentException>(() => spy.MapDelete(() => { }, "test"));
    }

    private void TestEndpointMethod() { }

    [Test]
    public void MapMethodsShouldNotThrowIfDelegateIsNotAnonymous()
    {
        var spy = GetRequiredService<EndpointRouteBuilderSpy>();

        Should.NotThrow(() => spy.MapGet(TestEndpointMethod));
        Should.NotThrow(() => spy.MapPost(TestEndpointMethod));
        Should.NotThrow(() => spy.MapPut(TestEndpointMethod, "test"));
        Should.NotThrow(() => spy.MapPatch(TestEndpointMethod, "test"));
        Should.NotThrow(() => spy.MapDelete(TestEndpointMethod, "test"));
    }

    /* TODO
    [Test]
    public void MapEndpointsShouldMapAllEndpointsFromAssembly()
    {
        var spy = GetRequiredService<EndpointRouteBuilderSpy>();
        spy.MapEndpointGroups(typeof(EndpointRouteBuilderExtensionTests).Assembly);

        var endpoints = spy.GetEndpoints();
        endpoints.Count.ShouldBe(5);

        var names = endpoints
            .Select(e => e.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName)
            .ToList();
        names.Count.ShouldBe(5);

        names.ShouldContain(nameof(TestEndpoints.Get));
        names.ShouldContain(nameof(TestEndpoints.Post));
        names.ShouldContain(nameof(TestEndpoints.Put));
        names.ShouldContain(nameof(TestEndpoints.Patch));
        names.ShouldContain(nameof(TestEndpoints.Delete));

        foreach (var endpoint in endpoints)
        {
            var tags = endpoint.Metadata.GetMetadata<ITagsMetadata>()!.Tags.ToArray();

            tags.Length.ShouldBe(1);
            tags.ShouldBeEquivalentTo(TestEndpoints.Tags);

            endpoint.DisplayName!.ShouldContain(TestEndpoints.Prefix);
        }
    }
    */

    [Test]
    public void MapLogoutEndpointShouldMapLogout()
    {
        var spy = GetRequiredService<EndpointRouteBuilderSpy>();

        spy.MapLogoutEndpoint();

        var endpoints = spy.GetEndpoints();
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
