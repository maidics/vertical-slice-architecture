using Microsoft.AspNetCore.Http.HttpResults;
using VsaTemplate.Common.Extensions;
using VsaTemplate.Common.Interfaces.Features;

namespace VsaTemplate.Features.Examples.Delete;

public sealed class DeleteExampleEndpoint : IEndpoint<Example>
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapDelete(DeleteExample, "");
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteExample(
        [AsParameters] DeleteExampleCommand command,
        DeleteExampleCommandHandler handler,
        CancellationToken cancellationToken
    )
    {
        var result = await handler.Handle(command, cancellationToken);

        return result.ToTypedResult();
    }
}
