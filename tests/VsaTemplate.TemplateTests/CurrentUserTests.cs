using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Shouldly;
using VsaTemplate.Common.Exceptions;
using VsaTemplate.Domain.Constants;
using VsaTemplate.Infrastructure;

namespace VsaTemplate.TemplateTests;

public sealed class CurrentUserTests
{
    [Test]
    [Arguments("not-a-guid")]
    [Arguments("")]
    [Arguments("42")]
    public void ConstructorShouldThrowIfNameIdentifierIsNotGuid(string nameIdentifier)
    {
        var accessor = CreateAccessor(new Claim(ClaimTypes.NameIdentifier, nameIdentifier));

        var ex = Should.Throw<InvalidNameIdentifierException>(() => new CurrentUser(accessor));
        ex.Message.ShouldContain(nameIdentifier);
    }

    [Test]
    public void ConstructorShouldNotThrowIfNameIdentifierIsGuid()
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

    private static HttpContextAccessor CreateAccessor(params Claim[] claims) =>
        new()
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test")),
            },
        };
}
