using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shouldly;
using VsaTemplate.Common.Pipeline;
using VsaTemplate.FunctionalTests.Infrastructure.TemplateTests;

namespace VsaTemplate.FunctionalTests.TemplateTests;

public sealed class ProblemDetailsExceptionHandlerTests
{
    [Test]
    public async Task TryHandleAsyncShouldWriteProblemDetailsAndReturnTrueOnBadHttpRequestException()
    {
        var logger = new TemplateTestLogger<ProblemDetailsExceptionHandler>();
        var handler = new ProblemDetailsExceptionHandler(logger);

        var body = new MemoryStream();
        var requestPath = "/test";
        var httpContext = new DefaultHttpContext
        {
            Response = { Body = body },
            Request = { Path = requestPath },
        };
        var exception = new BadHttpRequestException("Test.");

        var result = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);
        result.ShouldBeTrue();

        var response = httpContext.Response;
        response.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);

        body.Seek(0, SeekOrigin.Begin);

        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(
            body,
            new JsonSerializerOptions(JsonSerializerDefaults.Web)
        );

        problem.ShouldNotBeNull();
        problem.Title.ShouldBe("Bad Request");
        problem.Detail.ShouldBe("The request contains invalid or malformed parameters.");
        problem.Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1";
        problem.Status.ShouldBe(StatusCodes.Status400BadRequest);
        problem.Instance.ShouldBe(requestPath);

        logger.Entries.Count.ShouldBe(1);
        logger.Entries[0].Level.ShouldBe(LogLevel.Warning);
        logger.Entries[0].Message.ShouldContain("Bad HTTP Request at");
    }

    [TestCase(typeof(InvalidOperationException))]
    [TestCase(typeof(ArgumentNullException))]
    [TestCase(typeof(OperationCanceledException))]
    public async Task TryHandleAsyncShouldWriteProblemDetailsAndReturnTrueOnOtherExceptions(
        Type exceptionType
    )
    {
        var logger = new TemplateTestLogger<ProblemDetailsExceptionHandler>();
        var handler = new ProblemDetailsExceptionHandler(logger);

        var body = new MemoryStream();
        var requestPath = "/test";
        var httpContext = new DefaultHttpContext
        {
            Response = { Body = body },
            Request = { Path = requestPath },
        };

        var exception = (Exception)Activator.CreateInstance(exceptionType)!;

        var result = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);
        result.ShouldBeTrue();

        var response = httpContext.Response;
        response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);

        body.Seek(0, SeekOrigin.Begin);

        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(
            body,
            new JsonSerializerOptions(JsonSerializerDefaults.Web)
        );

        problem.ShouldNotBeNull();
        problem.Title.ShouldBe("Internal Server Error");
        problem.Detail.ShouldBe("An unexpected error occurred.");
        problem.Type.ShouldBe(
            "https://datatracker.ietf.org/doc/html/rfc9110#name-500-internal-server-error"
        );
        problem.Status.ShouldBe(StatusCodes.Status500InternalServerError);
        problem.Instance.ShouldBe(requestPath);

        logger.Entries.Count.ShouldBe(1);
        logger.Entries[0].Level.ShouldBe(LogLevel.Error);
        logger
            .Entries[0]
            .Message.ShouldContain("Unhandled exception caught while processing request at");
    }
}
