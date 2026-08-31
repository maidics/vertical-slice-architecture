using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VsaTemplate.Common.Pipeline;
using VsaTemplate.Tests.TestInfrastructure;
using VsaTemplate.Tests.TestInfrastructure.FunctionalTests;

namespace VsaTemplate.Tests.Common.Pipeline;

public sealed class ProblemDetailsExceptionHandlerTests : FunctionalTestBase
{
    [Test]
    [Arguments(StatusCodes.Status400BadRequest, "Bad Request")]
    [Arguments(StatusCodes.Status413PayloadTooLarge, "Content Too Large")]
    public async Task TryHandleAsyncShouldWriteProblemDetailsAndReturnTrueOnBadHttpRequestException(
        int statusCode,
        string expectedTitle
    )
    {
        var problemDetailsService = GetRequiredService<IProblemDetailsService>();
        var logger = new LoggerSpy<ProblemDetailsExceptionHandler>();
        var handler = new ProblemDetailsExceptionHandler(logger, problemDetailsService);

        var body = new MemoryStream();
        const string requestPath = "/test";
        var httpContext = new DefaultHttpContext
        {
            Response = { Body = body },
            Request = { Path = requestPath },
        };
        var exception = new BadHttpRequestException("Test.", statusCode);

        var result = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);
        result.ShouldBeTrue();

        var response = httpContext.Response;
        response.StatusCode.ShouldBe(statusCode);

        body.Seek(0, SeekOrigin.Begin);

        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(
            body,
            new JsonSerializerOptions(JsonSerializerDefaults.Web)
        );

        problem.ShouldNotBeNull();
        problem.Title.ShouldBe(expectedTitle);
        problem.Status.ShouldBe(statusCode);
        problem.Instance.ShouldBe(requestPath);

        logger.Entries.Count.ShouldBe(1);
        logger.Entries[0].Level.ShouldBe(LogLevel.Warning);
        logger.Entries[0].Message.ShouldContain("Bad HTTP Request at");
    }

    [Test]
    public async Task TryHandleAsyncShouldWriteProblemDetailsAndReturnTrueOnUnauthorizedAccessException()
    {
        var problemDetailsService = GetRequiredService<IProblemDetailsService>();
        var logger = new LoggerSpy<ProblemDetailsExceptionHandler>();
        var handler = new ProblemDetailsExceptionHandler(logger, problemDetailsService);

        var body = new MemoryStream();
        const string requestPath = "/test";
        var httpContext = new DefaultHttpContext
        {
            Response = { Body = body },
            Request = { Path = requestPath },
        };
        var exception = new UnauthorizedAccessException();

        var result = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);
        result.ShouldBeTrue();

        var response = httpContext.Response;
        response.StatusCode.ShouldBe(StatusCodes.Status401Unauthorized);

        body.Seek(0, SeekOrigin.Begin);

        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(
            body,
            new JsonSerializerOptions(JsonSerializerDefaults.Web)
        );

        problem.ShouldNotBeNull();
        problem.Instance.ShouldBe(requestPath);

        logger.Entries.Count.ShouldBe(1);
        logger.Entries[0].Level.ShouldBe(LogLevel.Error);
        logger.Entries[0].Message.ShouldContain("Unauthorized HTTP Request at");
    }

    [Test]
    [Arguments(typeof(InvalidOperationException))]
    [Arguments(typeof(ArgumentNullException))]
    [Arguments(typeof(OperationCanceledException))]
    public async Task TryHandleAsyncShouldWriteProblemDetailsAndReturnTrueOnOtherExceptions(
        Type exceptionType
    )
    {
        var problemDetailsService = GetRequiredService<IProblemDetailsService>();
        var logger = new LoggerSpy<ProblemDetailsExceptionHandler>();
        var handler = new ProblemDetailsExceptionHandler(logger, problemDetailsService);

        var body = new MemoryStream();
        const string requestPath = "/test";
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

    [Test]
    public async Task TryHandleAsyncShouldReturnTrueWithoutBodyWhenClientAborted()
    {
        var problemDetailsService = GetRequiredService<IProblemDetailsService>();
        var logger = new LoggerSpy<ProblemDetailsExceptionHandler>();
        var handler = new ProblemDetailsExceptionHandler(logger, problemDetailsService);

        var body = new MemoryStream();
        const string requestPath = "/test";
        var httpContext = new DefaultHttpContext
        {
            Response = { Body = body },
            Request = { Path = requestPath },
        };

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        httpContext.RequestAborted = cts.Token;

        var result = await handler.TryHandleAsync(
            httpContext,
            new OperationCanceledException(),
            CancellationToken.None
        );

        result.ShouldBeTrue();
        httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status499ClientClosedRequest);
        body.Length.ShouldBe(0);
        logger.Entries.Count.ShouldBe(0);
    }
}
