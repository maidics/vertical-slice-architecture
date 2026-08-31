using VsaTemplate.Common.Interfaces.Features;
using VsaTemplate.Common.Models;
using VsaTemplate.Infrastructure.Database;

namespace VsaTemplate.Features.Examples.AppendContent;

public sealed class AppendExampleContentCommandHandler : IRequestHandler
{
    private readonly ApplicationDbContext _context;

    public AppendExampleContentCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(
        AppendExampleContentCommand command,
        CancellationToken cancellationToken
    )
    {
        var example = await _context.Examples.FirstOrDefaultAsync(
            x => x.Id == command.ExampleId,
            cancellationToken
        );

        if (example is null)
            return Result.NotFound($"{nameof(Example)} not found.");

        var existing = await _context
            .Examples.AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Content == example.Content + command.AdditionalContent,
                cancellationToken
            );

        if (existing is not null)
            return Result.Conflict([
                $"Example with '{example.Content + command.AdditionalContent}' content already exists.",
            ]);

        example.AppendContent(command.AdditionalContent);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
