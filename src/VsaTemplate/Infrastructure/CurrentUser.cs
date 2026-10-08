using System.Collections.Frozen;
using System.Security.Claims;
using VsaTemplate.Common.Exceptions;
using VsaTemplate.Common.Interfaces;

namespace VsaTemplate.Infrastructure;

// Claims are read on access, not in the constructor: this scoped service can be created before
// authentication has populated HttpContext.User (e.g. Identity's SecurityStampValidator resolves
// the DbContext, and therefore its interceptors, while authenticating the request).
public sealed class CurrentUser : IUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? Id => ParseNameIdentifier(Principal?.FindFirstValue(ClaimTypes.NameIdentifier));

    public FrozenSet<string> Roles =>
        Principal?.FindAll(ClaimTypes.Role).Select(x => x.Value).ToFrozenSet() ?? [];

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    private static Guid? ParseNameIdentifier(string? nameIdentifier)
    {
        if (nameIdentifier is null)
            return null;

        if (!Guid.TryParse(nameIdentifier, out var id))
        {
            throw new InvalidNameIdentifierException(nameIdentifier);
        }

        return id;
    }
}
