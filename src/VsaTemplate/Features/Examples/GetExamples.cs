using Microsoft.AspNetCore.Http.HttpResults;
using VsaTemplate.Common.Extensions;
using VsaTemplate.Common.Interfaces;
using VsaTemplate.Infrastructure.Database;

namespace VsaTemplate.Features.Examples;

public sealed class GetExamplesQueryHandler : IRequestHandler
{
    private readonly ApplicationDbContext _context;

    public GetExamplesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ExampleDto>> Handle(CancellationToken cancellationToken)
    {
        return await _context
            .Examples.AsNoTracking()
            .Select(x => new ExampleDto(x.Id, x.Content, x.HasAppendedContent))
            .ToListAsync(cancellationToken);
    }
}

public sealed class GetExamplesEndpoint : IEndpoint
{
    public static string Prefix => "examples";

    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapGet(GetExamples);
    }

    private static async Task<Ok<List<ExampleDto>>> GetExamples(
        GetExamplesQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var result = await handler.Handle(cancellationToken);

        return TypedResults.Ok(result);
    }
}
