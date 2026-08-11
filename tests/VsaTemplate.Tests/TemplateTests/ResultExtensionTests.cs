using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Shouldly;
using VsaTemplate.Common.Extensions;
using VsaTemplate.Common.Models;

namespace VsaTemplate.Tests.TemplateTests;

public sealed class ResultExtensionTests
{
    [Test]
    public void ToTypedResultShouldReturnNoContentOnSucceededResult()
    {
        var result = Result.Success();

        var results = result.ToTypedResult();
        results.Result.ShouldBeOfType(typeof(NoContent));

        var noContent = (NoContent)results.Result;
        noContent.StatusCode.ShouldBe(StatusCodes.Status204NoContent);
    }

    [Test]
    public void ToTypedResultShouldReturnOkOnSucceededGenericResult()
    {
        var result = Result.Success("test");

        var results = result.ToTypedResult();
        results.Result.ShouldBeOfType(typeof(Ok<string>));

        var ok = (Ok<string>)results.Result;
        ok.StatusCode.ShouldBe(StatusCodes.Status200OK);
    }

    [TestCaseSource(nameof(ToTypedResultShouldReturnCorrectProblemDetailsForFailedResultSource))]
    public void ToTypedResultShouldReturnCorrectProblemDetailsForFailedResult(
        (
            Func<string[], ResultFailure> factoryMethod,
            int expectedStatus,
            string expectedTitle
        ) tuple
    )
    {
        var result = (Result)tuple.factoryMethod.Invoke(["test"]);
        var results = result.ToTypedResult();
        results.Result.ShouldBeOfType(typeof(ProblemHttpResult));

        var problem = (ProblemHttpResult)results.Result;
        problem.StatusCode.ShouldBe(tuple.expectedStatus);

        var problemDetails = problem.ProblemDetails;
        problemDetails.Status.ShouldBe(tuple.expectedStatus);
        problemDetails.Title.ShouldBe(tuple.expectedTitle, StringComparer.Ordinal);
        problemDetails.Extensions["errors"].ShouldBe(new[] { "test" });
    }

    public static IEnumerable<(
        Func<string[], ResultFailure> factoryMethod,
        int expectedStatus,
        string expectedTitle
    )> ToTypedResultShouldReturnCorrectProblemDetailsForFailedResultSource()
    {
        yield return (
            Result.Canceled,
            StatusCodes.Status499ClientClosedRequest,
            "Request Canceled"
        );
        yield return (Result.Timeout, StatusCodes.Status504GatewayTimeout, "Gateway Timeout");
        yield return (Result.NotFound, StatusCodes.Status404NotFound, "Not Found");
        yield return (Result.Conflict, StatusCodes.Status409Conflict, "Conflict");
        yield return (
            Result.ExternalServiceError,
            StatusCodes.Status503ServiceUnavailable,
            "Service Unavailable"
        );
        yield return (Result.RuleViolation, StatusCodes.Status400BadRequest, "Bad Request");
        yield return (
            Result.PaymentRequired,
            StatusCodes.Status402PaymentRequired,
            "Payment Required"
        );
        yield return (Result.Unauthorized, StatusCodes.Status401Unauthorized, "Unauthorized");
        yield return (Result.Forbidden, StatusCodes.Status403Forbidden, "Forbidden");
    }
}
