using System.Collections.Frozen;
using VsaTemplate.Common.Interfaces;

namespace VsaTemplate.Tests.TestInfrastructure.TemplateTests;

public sealed class TemplateTestUser : IUser
{
    public Guid? Id { get; private set; }
    public FrozenSet<string>? Roles { get; private set; }

    public Guid LogIn(Guid id, FrozenSet<string>? roles)
    {
        Id = id;
        Roles = roles;

        return id;
    }

    public void LogOut()
    {
        Id = null;
        Roles = null;
    }
}
