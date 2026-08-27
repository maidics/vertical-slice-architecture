using System.Collections.Frozen;
using VsaTemplate.Common.Interfaces;

namespace VsaTemplate.TemplateTests.Infrastructure;

public sealed class TestUser : IUser
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
