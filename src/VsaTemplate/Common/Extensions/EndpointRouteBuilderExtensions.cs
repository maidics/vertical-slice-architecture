using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using VsaTemplate.Common.Interfaces.Features;
using VsaTemplate.Features.Users;

namespace VsaTemplate.Common.Extensions;

// credit: https://github.com/jasontaylordev/CleanArchitecture
public static class EndpointRouteBuilderExtensions
{
    private static void ThrowIfMethodAnonymous(Delegate handler)
    {
        if (handler.Method.IsAnonymous())
            throw new ArgumentException(
                "The endpoint name must be specified when using anonymous handlers."
            );
    }

    extension(IEndpointRouteBuilder builder)
    {
        // this consumes the assembly so it is testable
        public IEndpointRouteBuilder MapEndpoints(Assembly assembly)
        {
            var endpointGroupTypes = assembly
                .GetExportedTypes()
                .Where(t =>
                    t is { IsAbstract: false, IsInterface: false }
                    && t.IsAssignableTo(typeof(IEndpointGroup))
                );

            foreach (var type in endpointGroupTypes)
            {
                var groupName =
                    type.GetProperty(nameof(IEndpointGroup.Prefix))!.GetValue(null) as string;

                var tags =
                    type.GetProperty(nameof(IEndpointGroup.Tags))!.GetValue(null) as string[];

                var group = builder.MapGroup($"/{groupName}").WithTags(tags!);
                type.GetMethod(nameof(IEndpointGroup.Map))!.Invoke(null, [group]);
            }

            return builder;
        }

        public IEndpointRouteBuilder MapLogoutEndpoint()
        {
            builder.MapPost("/identity/logout", Logout).WithTags("Users").RequireAuthorization();

            return builder;

            static async Task<Ok> Logout(SignInManager<ApplicationUser> signInManager)
            {
                await signInManager.SignOutAsync();
                return TypedResults.Ok();
            }
        }

        // these set method names (useful for client type generation)

        public RouteHandlerBuilder MapGet(
            Delegate handler,
            [StringSyntax("Route")] string pattern = ""
        )
        {
            ThrowIfMethodAnonymous(handler);

            return builder.MapGet(pattern, handler).WithName(handler.Method.Name);
        }

        public RouteHandlerBuilder MapPost(
            Delegate handler,
            [StringSyntax("Route")] string pattern = ""
        )
        {
            ThrowIfMethodAnonymous(handler);

            return builder.MapPost(pattern, handler).WithName(handler.Method.Name);
        }

        public RouteHandlerBuilder MapPut(Delegate handler, [StringSyntax("Route")] string pattern)
        {
            ThrowIfMethodAnonymous(handler);

            return builder.MapPut(pattern, handler).WithName(handler.Method.Name);
        }

        public RouteHandlerBuilder MapPatch(
            Delegate handler,
            [StringSyntax("Route")] string pattern
        )
        {
            ThrowIfMethodAnonymous(handler);

            return builder.MapPatch(pattern, handler).WithName(handler.Method.Name);
        }

        public RouteHandlerBuilder MapDelete(
            Delegate handler,
            [StringSyntax("Route")] string pattern
        )
        {
            ThrowIfMethodAnonymous(handler);

            return builder.MapDelete(pattern, handler).WithName(handler.Method.Name);
        }
    }
}
