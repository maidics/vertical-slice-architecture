using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using VsaTemplate.Common.Extensions;
using VsaTemplate.Common.Interfaces;
using VsaTemplate.Common.Models;
using VsaTemplate.Domain.Constants;
using VsaTemplate.Domain.Entities;
using VsaTemplate.Infrastructure.Database;

namespace VsaTemplate.Features.Examples;

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
            return Result.NotFound($"{nameof(Example)} not found.");

        if (example.Content == command.Content)
            return Result.Success();

        var existingWithContent = await _context
            .Examples.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Content == command.Content, cancellationToken);

        if (existingWithContent is not null)
            return Result.Conflict([
                $"{nameof(Example)} with '{command.Content}' content already exists.",
            ]);

        example.Content = command.Content;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}

public sealed class UpdateExampleEndpoint : IEndpoint
{
    public static string Prefix => "examples";

    public static void Map(IEndpointRouteBuilder builder)
    {
        builder
            .MapPut(UpdateExample, "")
            .RequireAuthorizationWithRoles(Roles.User, Roles.Administrator);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> UpdateExample(
        UpdateExampleCommandHandler handler,
        UpdateExampleCommand command,
        CancellationToken cancellationToken
    )
    {
        var result = await handler.Handle(command, cancellationToken);

        return result.ToTypedResult();
    }
}
