using Microsoft.AspNetCore.Authorization;
using VsaTemplate.Common.Constants;

namespace VsaTemplate.Common.Extensions;

public static class RouteHandlerBuilderExtensions
{
    extension(RouteHandlerBuilder builder)
    {
        public RouteHandlerBuilder RequireAuthorizationWithRoles(params string[] roles)
        {
            var invalid = roles.Where(r => !Roles.IsValid(r)).ToList();

            if (invalid.Count > 0)
                throw new ArgumentException($"Invalid role(s): {string.Join(", ", invalid)}");

            return builder.RequireAuthorization(
                new AuthorizeAttribute() { Roles = string.Join(",", roles) }
            );
        }
    }
}
