using Shouldly;
using VsaTemplate.Features.Users;

namespace VsaTemplate.FunctionalTests.FeatureTests.Users;

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
