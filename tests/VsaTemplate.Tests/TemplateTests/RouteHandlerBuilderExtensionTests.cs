using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Shouldly;
using VsaTemplate.Common.Extensions;
using VsaTemplate.Tests.Infrastructure.Common;

namespace VsaTemplate.Tests.TemplateTests;

public sealed class RouteHandlerBuilderExtensionTests : TemplateTestBase
{
    [TestCase]
    [TestCase("user")]
    [TestCase("user", "admin")]
    public void RequireAuthorizationWithRoleShouldApplyAuthorizationAttributeWithGivenRoles(
        params string[] roles
    )
    {
        _routeBuilder.MapGet("/test", () => { }).RequireAuthorizationWithRole(roles);

        var endpoints = _routeBuilder.GetEndpoints();
        endpoints.Count.ShouldBe(1);

        var endpoint = endpoints.First();
        var authMetadata = endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>();
        authMetadata.ShouldNotBeEmpty();
        authMetadata.Count.ShouldBe(1);
        authMetadata.First().Roles.ShouldBe(string.Join(",", roles));
    }
}
