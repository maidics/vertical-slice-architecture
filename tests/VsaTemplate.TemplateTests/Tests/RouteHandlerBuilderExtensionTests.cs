using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using VsaTemplate.Common.Extensions;
using VsaTemplate.TemplateTests.Infrastructure;
using VsaTemplate.TemplateTests.Infrastructure.Common.BaseClasses;

namespace VsaTemplate.TemplateTests.Tests;

public sealed class RouteHandlerBuilderExtensionTests : TestBase
{
    [Test]
    [Arguments]
    [Arguments("user")]
    [Arguments("user", "admin")]
    public void RequireAuthorizationWithRoleShouldApplyAuthorizationAttributeWithGivenRoles(
        params string[] roles
    )
    {
        var spy = GetRequiredService<EndpointRouteBuilderSpy>();

        spy.MapGet("/test", () => { }).RequireAuthorizationWithRole(roles);

        var endpoints = spy.GetEndpoints();
        endpoints.Count.ShouldBe(1);

        var endpoint = endpoints.First();
        var authMetadata = endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>();
        authMetadata.ShouldNotBeEmpty();
        authMetadata.Count.ShouldBe(1);
        authMetadata.First().Roles.ShouldBe(string.Join(",", roles));
    }
}
