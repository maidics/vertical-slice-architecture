using Microsoft.AspNetCore.Http.HttpResults;
using VsaTemplate.Common.Extensions;
using VsaTemplate.Common.Interfaces.Features;

namespace VsaTemplate.Features.Examples.AppendContent;

public sealed class AppendExampleContentEndpoint : IEndpoint
{
    public static string Prefix => "Example";
    public static string[] Tags => [Prefix];

    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapPut(AppendExampleContent, "append-content");
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> AppendExampleContent(
        AppendExampleContentCommandHandler handler,
        AppendExampleContentCommand command,
        CancellationToken cancellationToken
    )
    {
        var result = await handler.Handle(command, cancellationToken);

        return result.ToTypedResult();
    }
}
