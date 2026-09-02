using Microsoft.AspNetCore.Http.HttpResults;
using VsaTemplate.Common.Extensions;
using VsaTemplate.Common.Interfaces;
using VsaTemplate.Common.Models;
using VsaTemplate.Domain.Entities;
using VsaTemplate.Infrastructure.Database;

namespace VsaTemplate.Features.Examples;

public sealed class GetExampleByIdQueryHandler : IRequestHandler
{
    private readonly ApplicationDbContext _context;

    public GetExampleByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ExampleDto>> Handle(
        Guid exampleId,
        CancellationToken cancellationToken
    )
    {
        var example = await _context
            .Examples.AsNoTracking()
            .Where(x => x.Id == exampleId)
            .Select(x => new ExampleDto(x.Id, x.Content, x.HasAppendedContent))
            .FirstOrDefaultAsync(cancellationToken);

        if (example is null)
            return Result.NotFound($"{nameof(Example)} not found.");

        return Result.Success(example);
    }
}

public sealed class GetExampleByIdEndpoint : IEndpoint
{
    public static string Prefix => "examples";

    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapGet(GetExampleById, "{exampleId:guid}");
    }

    private static async Task<Results<Ok<ExampleDto>, ProblemHttpResult>> GetExampleById(
        Guid exampleId,
        GetExampleByIdQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var result = await handler.Handle(exampleId, cancellationToken);

        return result.ToTypedResult();
    }
}
