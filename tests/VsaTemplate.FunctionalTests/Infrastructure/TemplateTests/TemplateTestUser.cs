using System.Collections.Frozen;
using VsaTemplate.Common.Interfaces;

namespace VsaTemplate.FunctionalTests.Infrastructure.TemplateTests;

public sealed class TemplateTestUser : IUser
{
    public Guid? Id { get; }
    public FrozenSet<string>? Roles { get; }

    public TemplateTestUser(Guid? id, FrozenSet<string>? roles)
    {
        Id = id;
        Roles = roles;
    }
}
