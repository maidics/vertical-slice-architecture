using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using VsaTemplate.Common.Extensions;
using VsaTemplate.Common.Interfaces.Features;
using VsaTemplate.Common.Models;
using VsaTemplate.Domain.Entities;
using VsaTemplate.Infrastructure.Database;

namespace VsaTemplate.Features.Examples;

public sealed record CreateExampleCommand(string Content) : IRequest;

public sealed class CreateExampleCommandValidator : AbstractValidator<CreateExampleCommand>
{
    public CreateExampleCommandValidator()
    {
        RuleFor(x => x.Content).NotEmpty();
    }
}

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

public sealed class CreateExampleEndpoint : IEndpoint
{
    public static string Prefix => "examples";

    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapPost(CreateExample);
    }

    private static async Task<Results<Ok<Guid>, ProblemHttpResult>> CreateExample(
        CreateExampleCommandHandler handler,
        CreateExampleCommand command,
        CancellationToken cancellationToken
    )
    {
        var result = await handler.Handle(command, cancellationToken);

        return result.ToTypedResult();
    }
}
