using Microsoft.AspNetCore.Http.HttpResults;
using VsaTemplate.Common.Extensions;
using VsaTemplate.Common.Interfaces.Features;

namespace VsaTemplate.Features.Examples.Update;

public sealed class UpdateExampleEndpoint : IEndpoint
{
    public static string Prefix => nameof(Example);
    public static string[] Tags => [];

    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapPut(UpdateExample, "");
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
