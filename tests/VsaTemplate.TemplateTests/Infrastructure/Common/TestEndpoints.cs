using Microsoft.AspNetCore.Routing;
using VsaTemplate.Common.Extensions;
using VsaTemplate.Common.Interfaces.Features;

namespace VsaTemplate.TemplateTests.Infrastructure.Common;

public sealed class TestEndpoints : IEndpointGroup
{
    public static string Prefix { get; } = nameof(TestEndpoints);
    public static string[] Tags { get; } = ["test"];

    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapGet(Get, "get-test");
        builder.MapPost(Post, "post-test");
        builder.MapPut(Put, "put-test");
        builder.MapPatch(Patch, "patch-test");
        builder.MapDelete(Delete, "delete-test");
    }

    public static void Get() { }

    public static void Post() { }

    public static void Put() { }

    public static void Patch() { }

    public static void Delete() { }
}
