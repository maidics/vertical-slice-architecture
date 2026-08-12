using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VsaTemplate.Common.Models;

namespace VsaTemplate.Common.Extensions;

public static class ResultExtensions
{
    extension<T>(Result<T> result)
    {
        public Results<Ok<T>, ProblemHttpResult> ToTypedResult()
        {
            if (result.Succeeded)
            {
                return TypedResults.Ok(result.Value);
            }

            return result.MapResultFailure();
        }
    }

    extension(Result result)
    {
        public Results<NoContent, ProblemHttpResult> ToTypedResult()
        {
            if (result.Succeeded)
            {
                return TypedResults.NoContent();
            }

            return result.MapResultFailure();
        }

        private ProblemHttpResult MapResultFailure()
        {
            return TypedResults.Problem(CreateProblemDetails(result));
        }
    }

    private static (int status, string title) GetStatusCodeAndTitle(this ResultType type)
    {
        return type switch
        {
            ResultType.Canceled => (StatusCodes.Status499ClientClosedRequest, "Request Canceled"),
            ResultType.Timeout => (StatusCodes.Status504GatewayTimeout, "Gateway Timeout"),
            ResultType.NotFound => (StatusCodes.Status404NotFound, "Not Found"),
            ResultType.Conflict => (StatusCodes.Status409Conflict, "Conflict"),
            ResultType.ExternalServiceError => (
                StatusCodes.Status503ServiceUnavailable,
                "Service Unavailable"
            ),
            ResultType.RuleViolation => (StatusCodes.Status400BadRequest, "Bad Request"),
            ResultType.InternalError => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error"
            ),
            ResultType.PaymentRequired => (
                StatusCodes.Status402PaymentRequired,
                "Payment Required"
            ),
            ResultType.Unauthorized => (StatusCodes.Status401Unauthorized, "Unauthorized"),
            ResultType.Forbidden => (StatusCodes.Status403Forbidden, "Forbidden"),
            _ => throw new InvalidOperationException($"No status code implemented for: {type}"),
        };
    }

    private static ProblemDetails CreateProblemDetails(Result result)
    {
        var tuple = result.Type.GetStatusCodeAndTitle();

        var problemDetails = new ProblemDetails { Status = tuple.status, Title = tuple.title };

        //RFC 7807 standard
        problemDetails.Extensions["errors"] = result.Errors;

        return problemDetails;
    }
}
