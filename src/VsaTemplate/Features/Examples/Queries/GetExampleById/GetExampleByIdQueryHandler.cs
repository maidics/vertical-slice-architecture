using VsaTemplate.Common.Interfaces.Features;
using VsaTemplate.Common.Models;
using VsaTemplate.Infrastructure.Database;

namespace VsaTemplate.Features.Examples.Queries.GetExampleById;

public sealed class GetExampleByIdQueryHandler : IRequestHandler
{
    private readonly ApplicationDbContext _context;

    public GetExampleByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ExampleDto>> Handle(
        GetExampleByIdQuery query,
        CancellationToken cancellationToken
    )
    {
        var example = await _context
            .Examples.AsNoTracking()
            .Where(x => x.Id == query.Id)
            .Select(x => new ExampleDto(x.Id, x.Content, x.HasAppendedContent))
            .FirstOrDefaultAsync(cancellationToken);

        if (example is null)
            return Result.NotFound(["Example not found."]);

        return Result.Success(example);
    }
}
