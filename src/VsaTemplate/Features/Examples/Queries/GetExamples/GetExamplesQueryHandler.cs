using VsaTemplate.Common.Interfaces.Features;
using VsaTemplate.Infrastructure.Database;

namespace VsaTemplate.Features.Examples.Queries.GetExamples;

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
