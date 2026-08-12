using FluentValidation;
using VsaTemplate.Common.Interfaces;
using VsaTemplate.Common.Interfaces.Features;

namespace VsaTemplate.Common.Pipeline;

/* This makes sense if the request object is not created inside the minimal API method but comes with the incoming request
   If you want to validate the request inside or after the Minimal API you have to inject its validator and validate manually.
*/

public sealed class ValidationFilter : IEndpointFilter
{
    private readonly ILogger<ValidationFilter> _logger;
    private readonly IUser _user;

    public ValidationFilter(ILogger<ValidationFilter> logger, IUser user)
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

        if (request is null)
        {
            return await next(context);
        }

        var type = request.GetType();
        var validatorType = typeof(IValidator<>).MakeGenericType(type);
        var validators = context
            .HttpContext.RequestServices.GetServices(validatorType)
            .Cast<IValidator>()
            .ToList();

        var validationContext = new ValidationContext<object>(request);

        if (validators.Count == 0)
        {
            return await next(context);
        }

        var cancellationToken = context.HttpContext.RequestAborted;

        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(validationContext, cancellationToken))
        );

        var failures = validationResults.Where(r => !r.IsValid).SelectMany(r => r.Errors).ToList();

        if (failures.Count == 0)
        {
            return await next(context);
        }

        var errorsDictionary = failures
            .GroupBy(x => x.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());

        _logger.LogWarning(
            "Request validation failed: {HttpMethod} {Path}, {@UserId}, {@ValidationErrors}",
            context.HttpContext.Request.Method,
            context.HttpContext.Request.Path.Value,
            _user.Id,
            errorsDictionary
        );

        return TypedResults.ValidationProblem(errorsDictionary);
    }
}
