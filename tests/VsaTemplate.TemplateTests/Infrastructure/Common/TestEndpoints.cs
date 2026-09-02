using Microsoft.AspNetCore.Routing;
using VsaTemplate.Common.Extensions;
using VsaTemplate.Common.Interfaces;

namespace VsaTemplate.TemplateTests.Infrastructure.Common;

public sealed class TestGetEndpoint : IEndpoint
{
    public static string Prefix => "Test";

    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapGet(Get);
    }

    public static void Get() { }
}

public sealed class TestPostEndpoint : IEndpoint
{
    public static string Prefix => "Test";

    public static void Map(IEndpointRouteBuilder builder)
    {
        builder.MapPost(Post);
    }

    public static void Post() { }
}
