using System.Reflection;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Routing;
using Shouldly;
using VsaTemplate.Common.Extensions;
using VsaTemplate.Tests.Infrastructure.Common;
using VsaTemplate.Tests.Infrastructure.TemplateTests;

namespace VsaTemplate.Tests.TemplateTests;

public sealed class EndpointRouteBuilderExtensionsTests : TemplateTestBase
{
    [Test]
    public void MapMethodsShouldThrowWhenDelegateIsAnonymous()
    {
        Should.Throw<ArgumentException>(() => _routeBuilder.MapGet(() => { }));
        Should.Throw<ArgumentException>(() => _routeBuilder.MapPost(() => { }));
        Should.Throw<ArgumentException>(() => _routeBuilder.MapPut(() => { }, "test"));
        Should.Throw<ArgumentException>(() => _routeBuilder.MapPatch(() => { }, "test"));
        Should.Throw<ArgumentException>(() => _routeBuilder.MapDelete(() => { }, "test"));
    }

    [TestCase("MapGet")]
    [TestCase("MapPost")]
    [TestCase("MapPut")]
    [TestCase("MapPatch")]
    [TestCase("MapDelete")]
    public void MapMethodsShouldNotThrowIfDelegateIsNotAnonymous(string method)
    {
        var mapMethod = typeof(EndpointRouteBuilderExtensions).GetMethod(
            method,
            BindingFlags.Public | BindingFlags.Static
        );
        ArgumentNullException.ThrowIfNull(mapMethod);

        mapMethod.Invoke(null, [_routeBuilder, (Delegate)EndpointMethod, "test"]);

        var endpoints = _routeBuilder.GetEndpoints();
        endpoints.Count.ShouldBe(1);
        var endpoint = endpoints.First();
        endpoint.DisplayName.ShouldNotBeNull();
        endpoint.DisplayName.ShouldContain(nameof(EndpointMethod), Case.Sensitive);
        endpoint.DisplayName.ShouldContain("test", Case.Sensitive);
        endpoint.DisplayName.ShouldContain(method.Replace("Map", string.Empty));

        var nameMetadata = endpoint.Metadata.GetMetadata<IEndpointNameMetadata>();
        nameMetadata.ShouldNotBeNull();
        nameMetadata.EndpointName.ShouldBe(nameof(EndpointMethod));

        var methodInfoMetadata = endpoint.Metadata.GetMetadata<MethodInfo>();
        methodInfoMetadata.ShouldNotBeNull();
        methodInfoMetadata.ShouldBe(((Delegate)EndpointMethod).Method);
    }

    public void EndpointMethod() { }

    [Test]
    public void MapEndpointsShouldMapAllEndpointsFromAssembly()
    {
        _routeBuilder.MapEndpoints(typeof(TemplateTestEndpointRouteBuilder).Assembly);

        var endpoints = _routeBuilder.GetEndpoints();
        endpoints.Count.ShouldBe(5);

        var names = endpoints
            .Select(e => e.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName)
            .ToList();
        names.Count.ShouldBe(5);

        names.ShouldContain(nameof(TemplateTestEndpoints.Get));
        names.ShouldContain(nameof(TemplateTestEndpoints.Post));
        names.ShouldContain(nameof(TemplateTestEndpoints.Put));
        names.ShouldContain(nameof(TemplateTestEndpoints.Patch));
        names.ShouldContain(nameof(TemplateTestEndpoints.Delete));

        foreach (var endpoint in endpoints)
        {
            var tags = endpoint.Metadata.GetMetadata<ITagsMetadata>()!.Tags.ToArray();

            tags.Length.ShouldBe(1);
            tags.ShouldBeEquivalentTo(TemplateTestEndpoints.Tags);

            endpoint.DisplayName!.ShouldContain(TemplateTestEndpoints.Prefix);
        }
    }

    [Test]
    public void MapLogoutEndpointShouldMapLogout()
    {
        _routeBuilder.MapLogoutEndpoint();

        var endpoints = _routeBuilder.GetEndpoints();
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
