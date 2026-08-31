using Microsoft.AspNetCore.Http.HttpResults;
using VsaTemplate.Common.Extensions;
using VsaTemplate.Common.Interfaces.Features;

namespace VsaTemplate.Features.Examples.GetById;

public sealed class GetExampleByIdEndpoint : IEndpoint
{
    public static string Prefix => "examples";

    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapGet(GetExampleById, "{exampleId:guid}");
    }

    private static async Task<Results<Ok<ExampleDto>, ProblemHttpResult>> GetExampleById(
        Guid exampleId,
        GetExampleByIdQueryHandler handler,
        CancellationToken cancellationToken
    )
    {
        var result = await handler.Handle(exampleId, cancellationToken);

        return result.ToTypedResult();
    }
}
