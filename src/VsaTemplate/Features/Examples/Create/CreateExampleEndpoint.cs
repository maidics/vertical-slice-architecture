using Microsoft.AspNetCore.Http.HttpResults;
using VsaTemplate.Common.Extensions;
using VsaTemplate.Common.Interfaces.Features;

namespace VsaTemplate.Features.Examples.Create;

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
