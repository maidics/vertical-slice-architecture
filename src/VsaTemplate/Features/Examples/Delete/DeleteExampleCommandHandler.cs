using VsaTemplate.Common.Interfaces.Features;
using VsaTemplate.Common.Models;
using VsaTemplate.Infrastructure.Database;

namespace VsaTemplate.Features.Examples.Delete;

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
            return Result.NotFound(["Example not found."]);

        _context.Examples.Remove(example);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
