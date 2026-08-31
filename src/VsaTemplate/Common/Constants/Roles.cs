using System.Collections.Frozen;

namespace VsaTemplate.Common.Constants;

public abstract class Roles
{
    public const string User = nameof(User);
    public const string Administrator = nameof(Administrator);

    public static readonly FrozenSet<string> All = [User, Administrator];

    public static bool IsValid(string role) => All.Contains(role);
}
