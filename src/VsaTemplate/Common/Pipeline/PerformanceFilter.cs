using System.Diagnostics;
using VsaTemplate.Common.Interfaces;
using VsaTemplate.Common.Interfaces.Features;

namespace VsaTemplate.Common.Pipeline;

public sealed class PerformanceFilter : IEndpointFilter
{
    private readonly ILogger<PerformanceFilter> _logger;
    private readonly IUser _user;

    public PerformanceFilter(ILogger<PerformanceFilter> logger, IUser user)
    {
        _logger = logger;
        _user = user;
    }

    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next
    )
    {
        var timer = Stopwatch.StartNew();

        var result = await next(context);

        timer.Stop();

        var elapsedMilliseconds = timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500)
        {
            var request = context.Arguments.OfType<IRequest>().FirstOrDefault();

            _logger.LogWarning(
                "Long running request: {HttpMethod} {Path}, {@UserId}, {@Request}, ({@ElapsedMilliseconds}ms)",
                context.HttpContext.Request.Method,
                context.HttpContext.Request.Path.Value,
                _user.Id,
                request,
                elapsedMilliseconds
            );
        }

        return result;
    }
}
