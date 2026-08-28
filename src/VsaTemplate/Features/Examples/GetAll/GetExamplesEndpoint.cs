using Microsoft.AspNetCore.Http.HttpResults;
using VsaTemplate.Common.Extensions;
using VsaTemplate.Common.Interfaces.Features;

namespace VsaTemplate.Features.Examples.GetAll;

public sealed class GetExamplesEndpoint : IEndpoint<Example>
{
    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapGet(GetExamples);
    }

    private static async Task<Ok<List<ExampleDto>>> GetExamples(
        GetExamplesQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var result = await handler.Handle(cancellationToken);

        return TypedResults.Ok(result);
    }
}
