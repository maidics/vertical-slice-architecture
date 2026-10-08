using VsaTemplate.Common.Interfaces;

namespace VsaTemplate.Tests.TestInfrastructure.FunctionalTests;

public sealed class FunctionalTestUser : IUser
{
    public Guid? Id { get; private set; }
    public IReadOnlyList<string>? Roles { get; private set; }

    public void LogIn(Guid id, IReadOnlyList<string>? roles)
    {
        Id = id;
        Roles = roles;
    }

    public void LogOut()
    {
        Id = null;
        Roles = null;
    }
}
