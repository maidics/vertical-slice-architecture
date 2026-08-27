using System.Collections.Frozen;
using VsaTemplate.Common.Interfaces;

namespace VsaTemplate.Tests.TestInfrastructure.FunctionalTests;

public sealed class FunctionalTestUser : IUser
{
    public Guid? Id { get; private set; }
    public FrozenSet<string>? Roles { get; private set; }

    public void LogIn(Guid id, FrozenSet<string>? roles)
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
