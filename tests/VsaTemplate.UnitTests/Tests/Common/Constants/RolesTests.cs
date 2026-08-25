using Shouldly;
using VsaTemplate.Common.Constants;
using VsaTemplate.Features.Users;

namespace VsaTemplate.UnitTests.Common.Constants;

public sealed class RolesTests
{
    [Test]
    public void IsValidShouldReturnTrueWhenRoleIsValid()
    {
        Roles.IsValid(Roles.User).ShouldBeTrue();
    }

    [Test]
    public void IsValidShouldReturnFalseWhenRoleIsNotValid()
    {
        Roles.IsValid("test").ShouldBeFalse();
    }
}
