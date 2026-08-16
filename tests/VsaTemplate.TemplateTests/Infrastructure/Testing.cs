using System.Collections.Frozen;

namespace VsaTemplate.TemplateTests.Infrastructure;

public sealed class Testing
{
    private static Guid? _userId;
    private static FrozenSet<string>? _roles;

    public static Guid? GetUserId() => _userId;

    public static FrozenSet<string>? GetRoles() => _roles;

    public static Guid LogUserIn(Guid userId, IEnumerable<string> roles)
    {
        _userId = userId;
        _roles = roles.ToFrozenSet();

        return userId;
    }

    public static void LogUserOut()
    {
        _userId = null;
        _roles = null;
    }

    public static async Task ResetState()
    {
        if (TestSetUpFixture.Database is not null)
            await TestSetUpFixture.Database.ResetAsync();

        LogUserOut();
    }
}
