using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using VsaTemplate.Common.Extensions;
using VsaTemplate.Common.Interfaces;
using VsaTemplate.Common.Models;
using VsaTemplate.Domain.Constants;
using VsaTemplate.Domain.Entities;
using VsaTemplate.Infrastructure.Database;

namespace VsaTemplate.Features.Examples;

public sealed record AppendExampleContentCommand(Guid ExampleId, string AdditionalContent)
    : IRequest;

public sealed class AppendExampleContentCommandValidator
    : AbstractValidator<AppendExampleContentCommand>
{
    public AppendExampleContentCommandValidator()
    {
        RuleFor(x => x.AdditionalContent).NotEmpty();
    }
}

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
                $"{nameof(Example)} with '{example.Content + command.AdditionalContent}' content already exists.",
            ]);

        example.AppendContent(command.AdditionalContent);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed class AppendExampleContentEndpoint : IEndpoint
{
    public static string Prefix => "examples";

    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapPatch(AppendExampleContent, "append-content")
            .RequireAuthorizationWithRoles([Roles.User, Roles.Administrator]);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> AppendExampleContent(
        AppendExampleContentCommand command,
        AppendExampleContentCommandHandler handler,
        CancellationToken cancellationToken
    )
    {
        var result = await handler.Handle(command, cancellationToken);

        return result.ToTypedResult();
    }
}
