using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace VsaTemplate.Common.Pipeline;

public sealed class ProblemDetailsExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ProblemDetailsExceptionHandler> _logger;

    public ProblemDetailsExceptionHandler(ILogger<ProblemDetailsExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        // Customize this for your own needs
        // See: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling
        // See: https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.diagnostics.iexceptionhandler.tryhandleasync

        // BadHttpRequestException thrown by framework
        var requestMethod = httpContext.Request.Method;
        var requestPath = httpContext.Request.Path;

        var problemDetails = exception switch
        {
            BadHttpRequestException ex => HandleBadHttpRequestException(
                ex,
                requestMethod,
                requestPath
            ),
            _ => HandleDefault(exception, requestMethod, requestPath),
        };

        httpContext.Response.StatusCode = problemDetails.Status!.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private ProblemDetails HandleBadHttpRequestException(
        BadHttpRequestException badHttpRequestException,
        string requestMethod,
        string requestPath
    )
    {
        // calling LogWarning because it's a client mistake and not a server error
        _logger.LogWarning(
            badHttpRequestException,
            "Bad HTTP Request at [{HttpMethod}] {Path}",
            requestMethod,
            requestPath
        );

        return new ProblemDetails
        {
            Status = badHttpRequestException.StatusCode,
            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1",
            Title = "Bad Request",
            Detail = "The request contains invalid or malformed parameters.",
            Instance = requestPath,
        };
    }

    private ProblemDetails HandleDefault(
        Exception exception,
        string requestMethod,
        string requestPath
    )
    {
        _logger.LogError(
            exception,
            "Unhandled exception caught while processing request at [{HttpMethod}] {Path}",
            requestMethod,
            requestPath
        );

        return new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Type = "https://datatracker.ietf.org/doc/html/rfc9110#name-500-internal-server-error",
            Title = "Internal Server Error",
            Detail = "An unexpected error occurred.",
            Instance = requestPath,
        };
    }
}
