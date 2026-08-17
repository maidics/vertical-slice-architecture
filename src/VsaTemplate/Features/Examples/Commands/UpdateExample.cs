using FluentValidation;
using VsaTemplate.Common.Interfaces.Features;
using VsaTemplate.Common.Models;
using VsaTemplate.Infrastructure.Database;

namespace VsaTemplate.Features.Examples.Commands;

public sealed record UpdateExampleCommand(Guid Id, string Content) : IRequest;

public sealed class UpdateExampleCommandValidator : AbstractValidator<UpdateExampleCommand>
{
    public UpdateExampleCommandValidator()
    {
        RuleFor(x => x.Content).NotEmpty();
    }
}

public sealed class UpdateExampleCommandHandler : IRequestHandler
{
    private readonly ApplicationDbContext _context;

    public UpdateExampleCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(
        UpdateExampleCommand command,
        CancellationToken cancellationToken
    )
    {
        var example = await _context.Examples.FirstOrDefaultAsync(
            x => x.Id == command.Id,
            cancellationToken
        );

        if (example is null)
            return Result.NotFound(["Example not found."]);

        if (example.Content == command.Content)
            return Result.Success();

        var existingWithContent = await _context
            .Examples.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Content == command.Content, cancellationToken);

        if (existingWithContent is not null)
            return Result.Conflict([$"Example with '{command.Content}' content already exists."]);

        example.Content = command.Content;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
