using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Shouldly;
using VsaTemplate.Common.Constants;
using VsaTemplate.Common.Extensions;
using VsaTemplate.TemplateTests.Infrastructure;
using VsaTemplate.TemplateTests.Infrastructure.Common.BaseClasses;

namespace VsaTemplate.TemplateTests;

public sealed class RouteHandlerBuilderExtensionTests : TestBase
{
    [Test]
    public void RequireAuthorizationWithRolesShouldThrowIfRolesIsEmpty()
    {
        var spy = GetRequiredService<EndpointRouteBuilderSpy>();

        Should.Throw<ArgumentOutOfRangeException>(() =>
            spy.MapGet("/test", () => { }).RequireAuthorizationWithRoles([])
        );
    }

    [Test]
    [Arguments("")]
    [Arguments("Admin")]
    [Arguments("aadminn")]
    [Arguments("Administrator", "Userr")]
    [Arguments("Administrator", "Userr", "user")]
    public void RequireAuthorizationWithRolesShouldThrowIfAnyRoleIsInvalid(params string[] roles)
    {
        var spy = GetRequiredService<EndpointRouteBuilderSpy>();

        var ex = Should.Throw<ArgumentException>(() =>
            spy.MapGet("/test", () => { }).RequireAuthorizationWithRoles(roles)
        );

        ex.Message.ShouldContain(string.Join(", ", roles.Where(r => !Roles.IsValid(r))));
    }

    [Test]
    [Arguments("User")]
    [Arguments("User", "Administrator")]
    public void RequireAuthorizationWithRolesShouldApplyAuthorizationAttributeWithGivenValidRoles(
        params string[] roles
    )
    {
        var spy = GetRequiredService<EndpointRouteBuilderSpy>();

        spy.MapGet("/test", () => { }).RequireAuthorizationWithRoles(roles);

        var endpoints = spy.GetEndpoints();
        endpoints.Count.ShouldBe(1);

        var endpoint = endpoints.First();
        var authMetadata = endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>();
        authMetadata.ShouldNotBeEmpty();
        authMetadata.Count.ShouldBe(1);
        authMetadata.First().Roles.ShouldBe(roles.Length == 0 ? null : string.Join(",", roles));
    }
}
