using VsaTemplate.Common.Constants;

namespace VsaTemplate.Tests.Common.Constants;

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
