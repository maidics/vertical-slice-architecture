using VsaTemplate.Domain.Constants;

namespace VsaTemplate.UnitTests.Domain.Constants;

public sealed class RolesTests
{
    [Test]
    [Arguments(Roles.User)]
    [Arguments(Roles.Administrator)]
    public void IsValidShouldReturnTrueWhenRoleIsValid(string role)
    {
        Roles.IsValid(role).ShouldBeTrue();
    }

    [Test]
    public void IsValidShouldReturnFalseWhenRoleIsNotValid()
    {
        Roles.IsValid("test").ShouldBeFalse();
    }
}
