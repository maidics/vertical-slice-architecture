using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Shouldly;
using VsaTemplate.Common.Extensions;
using VsaTemplate.Domain.Constants;
using VsaTemplate.TemplateTests.Infrastructure.Common.BaseClasses;
using VsaTemplate.Tests.TestInfrastructure.WebTests;

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
    [Arguments(Roles.User)]
    [Arguments(Roles.User, Roles.Administrator)]
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
        authMetadata[0].Roles.ShouldBe(string.Join(",", roles));
    }
}
