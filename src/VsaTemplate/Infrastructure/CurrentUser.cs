using System.Security.Claims;
using VsaTemplate.Common.Exceptions;
using VsaTemplate.Common.Interfaces;

namespace VsaTemplate.Infrastructure;

// Read claims on access: Identity's SecurityStampValidator can create this before HttpContext.User is set.
public sealed class CurrentUser : IUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? Id => ParseNameIdentifier(Principal?.FindFirstValue(ClaimTypes.NameIdentifier));

    public IReadOnlyList<string> Roles =>
        Principal?.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList() ?? [];

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
