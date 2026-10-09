using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using VsaTemplate.Common.Exceptions;
using VsaTemplate.Common.Pipeline;
using VsaTemplate.Tests.TestInfrastructure;
using VsaTemplate.Tests.TestInfrastructure.FunctionalTests;

namespace VsaTemplate.Tests.Common.Pipeline;

public sealed class ProblemDetailsExceptionHandlerTests : FunctionalTestBase
{
    private readonly FakeLogger<ProblemDetailsExceptionHandler> _logger = new();

    [Test]
    [Arguments(StatusCodes.Status400BadRequest, "Bad Request")]
    [Arguments(StatusCodes.Status413PayloadTooLarge, "Content Too Large")]
    public async Task TryHandleAsyncShouldWriteProblemDetailsAndReturnTrueOnBadHttpRequestException(
        int statusCode,
        string expectedTitle
    )
    {
        var problemDetailsService = GetRequiredService<IProblemDetailsService>();
        var handler = new ProblemDetailsExceptionHandler(_logger, problemDetailsService);

        var body = new MemoryStream();

        const string httpMethod = "POST";
        const string path = "/test";

        var httpContext = new DefaultHttpContext
        {
            Request = { Method = httpMethod, Path = new PathString(path) },
            Response = { Body = body },
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
        problem.Instance.ShouldBe(path);

        _logger.Collector.Count.ShouldBe(1);
        _logger.Collector.LatestRecord.Level.ShouldBe(LogLevel.Warning);
        _logger.Collector.LatestRecord.Message.ShouldContain(
            $"Bad HTTP Request at [{httpMethod}] {path}"
        );
    }

    [Test]
    public async Task TryHandleAsyncShouldWriteProblemDetailsAndReturnTrueOnInvalidNameIdentifierException()
    {
        var problemDetailsService = GetRequiredService<IProblemDetailsService>();
        var handler = new ProblemDetailsExceptionHandler(_logger, problemDetailsService);

        var body = new MemoryStream();

        const string httpMethod = "POST";
        const string path = "/test";

        var httpContext = new DefaultHttpContext
        {
            Response = { Body = body },
            Request = { Method = httpMethod, Path = new PathString(path) },
        };
        var exception = new InvalidNameIdentifierException("test");

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
        problem.Instance.ShouldBe(path);

        _logger.Collector.Count.ShouldBe(1);
        _logger.Collector.LatestRecord.Level.ShouldBe(LogLevel.Error);
        _logger.Collector.LatestRecord.Message.ShouldContain(
            $"HTTP Request contains invalid name identifier claim at [{httpMethod}] {path}"
        );
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
        var handler = new ProblemDetailsExceptionHandler(_logger, problemDetailsService);

        var body = new MemoryStream();

        const string httpMethod = "POST";
        const string path = "/test";

        var httpContext = new DefaultHttpContext
        {
            Response = { Body = body },
            Request = { Method = httpMethod, Path = new PathString(path) },
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
        problem.Instance.ShouldBe(path);

        _logger.Collector.Count.ShouldBe(1);
        _logger.Collector.LatestRecord.Level.ShouldBe(LogLevel.Error);
        _logger.Collector.LatestRecord.Message.ShouldContain(
            $"Unhandled exception caught while processing request at [{httpMethod}] {path}"
        );
    }

    [Test]
    public async Task TryHandleAsyncShouldReturnTrueWithoutBodyWhenClientAborted()
    {
        var problemDetailsService = GetRequiredService<IProblemDetailsService>();
        var handler = new ProblemDetailsExceptionHandler(_logger, problemDetailsService);

        var body = new MemoryStream();

        const string path = "/test";

        var httpContext = new DefaultHttpContext
        {
            Response = { Body = body },
            Request = { Path = new PathString(path) },
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
        _logger.Collector.Count.ShouldBe(0);
    }
}
