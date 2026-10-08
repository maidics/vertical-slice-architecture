using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using VsaTemplate.Common.Exceptions;
using VsaTemplate.Domain.Constants;
using VsaTemplate.Infrastructure;

namespace VsaTemplate.Tests.Infrastructure;

public sealed class CurrentUserTests
{
    [Test]
    [Arguments("not-a-guid")]
    [Arguments("")]
    [Arguments("42")]
    public void IdShouldThrowIfNameIdentifierIsNotGuid(string nameIdentifier)
    {
        var accessor = CreateAccessor(new Claim(ClaimTypes.NameIdentifier, nameIdentifier));

        var user = new CurrentUser(accessor);

        var ex = Should.Throw<InvalidNameIdentifierException>(() => user.Id);
        ex.Message.ShouldContain(nameIdentifier);
    }

    [Test]
    public void ShouldReturnIdAndRolesIfNameIdentifierIsGuid()
    {
        var userId = Guid.NewGuid();

        var accessor = CreateAccessor(
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, Roles.User)
        );

        var user = new CurrentUser(accessor);
        user.Id.ShouldBe(userId);
        user.Roles.ShouldBe([Roles.User]);
    }

    [Test]
    public void ShouldReturnNoIdAndRolesIfThereIsNoHttpContext()
    {
        var user = new CurrentUser(new HttpContextAccessor());

        user.Id.ShouldBeNull();
        user.Roles.ShouldBeEmpty();
    }

    [Test]
    public void ShouldReturnIdAndRolesIfUserIsAuthenticatedAfterConstruction()
    {
        var accessor = CreateAccessor();

        var user = new CurrentUser(accessor);
        user.Id.ShouldBeNull();
        user.Roles.ShouldBeEmpty();

        var userId = Guid.NewGuid();
        accessor.HttpContext!.User = CreatePrincipal(
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, Roles.User)
        );

        user.Id.ShouldBe(userId);
        user.Roles.ShouldBe([Roles.User]);
    }

    private static HttpContextAccessor CreateAccessor(params Claim[] claims) =>
        new() { HttpContext = new DefaultHttpContext { User = CreatePrincipal(claims) } };

    private static ClaimsPrincipal CreatePrincipal(params Claim[] claims) =>
        new(new ClaimsIdentity(claims, "Test"));
}
