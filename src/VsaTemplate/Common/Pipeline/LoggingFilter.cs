using VsaTemplate.Common.Interfaces;

namespace VsaTemplate.Common.Pipeline;

public sealed class LoggingFilter : IEndpointFilter
{
    private readonly ILogger<LoggingFilter> _logger;
    private readonly IUser _user;

    public LoggingFilter(ILogger<LoggingFilter> logger, IUser user)
    {
        _logger = logger;
        _user = user;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        var request = context.Arguments.OfType<IRequest>().FirstOrDefault();

        var result = await next(context);
        var statusCode = result is IResult r ? GetStatusCode(r) : null;

        _logger.LogInformation(
            "Request: {HttpMethod} {Path}, {@UserId}, {@Request}, {@ResponseStatusCode}",
            context.HttpContext.Request.Method,
            context.HttpContext.Request.Path.Value,
            _user.Id,
            request is null ? "none" : request,
            statusCode
        );

        return result;
    }

    private static int? GetStatusCode(IResult result) =>
        result switch
        {
            INestedHttpResult nested => GetStatusCode(nested.Result),
            IStatusCodeHttpResult statusCodeResult => statusCodeResult.StatusCode,
            _ => null,
        };
}
