using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using VsaTemplate.Common.Extensions;
using VsaTemplate.Domain.Constants;
using VsaTemplate.Tests.TestInfrastructure.TemplateTests;
using VsaTemplate.Tests.TestInfrastructure.WebTests;

namespace VsaTemplate.Tests.Common.Extensions;

public sealed class RouteHandlerBuilderExtensionTests
{
    [ClassDataSource<EndpointRouteBuilderSpy>]
    public required EndpointRouteBuilderSpy Spy { get; init; }

    [Test]
    public void RequireAuthorizationWithRolesShouldThrowIfRolesIsEmpty()
    {
        Should.Throw<ArgumentOutOfRangeException>(() =>
            Spy.MapGet("/test", () => { }).RequireAuthorizationWithRoles([])
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
        var ex = Should.Throw<ArgumentException>(() =>
            Spy.MapGet("/test", () => { }).RequireAuthorizationWithRoles(roles)
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
        Spy.MapGet("/test", () => { }).RequireAuthorizationWithRoles(roles);

        var endpoints = Spy.GetEndpoints();
        endpoints.Count.ShouldBe(1);

        var endpoint = endpoints.First();
        var authMetadata = endpoint.Metadata.GetOrderedMetadata<IAuthorizeData>();
        authMetadata.ShouldNotBeEmpty();
        authMetadata.Count.ShouldBe(1);
        authMetadata[0].Roles.ShouldBe(string.Join(",", roles));
    }
}
