using System.Collections.Frozen;
using System.Security.Claims;
using VsaTemplate.Common.Interfaces;

namespace VsaTemplate.Infrastructure;

public sealed class CurrentUser : IUser
{
    public Guid? Id { get; }
    public FrozenSet<string> Roles { get; } = [];

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        if (httpContextAccessor.HttpContext is null)
        {
            return;
        }

        var nameIdentifier = httpContextAccessor.HttpContext.User.FindFirstValue(
            ClaimTypes.NameIdentifier
        );

        Id = ParseNameIdentifier(nameIdentifier);

        Roles = httpContextAccessor
            .HttpContext.User.FindAll(ClaimTypes.Role)
            .Select(x => x.Value)
            .ToFrozenSet();
    }

    private Guid? ParseNameIdentifier(string? nameIdentifier)
    {
        if (nameIdentifier is null)
            return null;

        if (!Guid.TryParse(nameIdentifier, out var id))
        {
            throw new UnauthorizedAccessException(
                $"Could not parse user id to {nameof(Guid)}: '{nameIdentifier}'"
            );
        }

        return id;
    }
}
