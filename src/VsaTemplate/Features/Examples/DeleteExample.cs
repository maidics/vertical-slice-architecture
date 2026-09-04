using Microsoft.AspNetCore.Http.HttpResults;
using VsaTemplate.Common.Extensions;
using VsaTemplate.Common.Interfaces;
using VsaTemplate.Common.Models;
using VsaTemplate.Domain.Constants;
using VsaTemplate.Domain.Entities;
using VsaTemplate.Infrastructure.Database;

namespace VsaTemplate.Features.Examples;

public sealed class DeleteExampleCommandHandler : IRequestHandler
{
    private readonly ApplicationDbContext _context;

    public DeleteExampleCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(Guid exampleId, CancellationToken cancellationToken)
    {
        var example = await _context.Examples.FirstOrDefaultAsync(
            x => x.Id == exampleId,
            cancellationToken
        );

        if (example is null)
            return Result.NotFound($"{nameof(Example)} not found.");

        _context.Examples.Remove(example);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed class DeleteExampleEndpoint : IEndpoint
{
    public static string Prefix => "examples";

    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapDelete(DeleteExample, "{exampleId:guid}")
            .RequireAuthorizationWithRoles(Roles.Administrator);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteExample(
        Guid exampleId,
        DeleteExampleCommandHandler handler,
        CancellationToken cancellationToken
    )
    {
        var result = await handler.Handle(exampleId, cancellationToken);

        return result.ToTypedResult();
    }
}
