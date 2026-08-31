using VsaTemplate.Common.Interfaces.Features;
using VsaTemplate.Common.Models;
using VsaTemplate.Infrastructure.Database;

namespace VsaTemplate.Features.Examples.Create;

public sealed class CreateExampleCommandHandler : IRequestHandler
{
    private readonly ApplicationDbContext _context;

    public CreateExampleCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Guid>> Handle(
        CreateExampleCommand command,
        CancellationToken cancellationToken
    )
    {
        var existing = await _context.Examples.FirstOrDefaultAsync(
            x => x.Content == command.Content,
            cancellationToken
        );

        if (existing is not null)
            return Result.Conflict(
                $"{nameof(Example)} already exists with content: {command.Content}"
            );

        var example = new Example { Content = command.Content };

        await _context.Examples.AddAsync(example, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(example.Id);
    }
}
