using Shouldly;
using VsaTemplate.Common.Models;
using VsaTemplate.Tests.Infrastructure.Common;

namespace VsaTemplate.Tests.TemplateTests;

public sealed class ResultTests
{
    [Test]
    public void SuccessMethodShouldCreateSucceededResult()
    {
        var result = Result.Success();
        result.ShouldBeSuccessful();
    }

    [Test]
    public void SuccessMethodShouldCreateSucceededGenericResult()
    {
        var result = Result.Success("test");
        result.ShouldBeSuccessful("test");
    }

    [Test]
    public void NotFoundMethodShouldCreateNotFoundResult()
    {
        var result = Result.NotFound("test");
        ((Result<string>)result).ShouldBeFailed(ResultType.NotFound, "test");
    }

    [TestCaseSource(nameof(FailureMethodsShouldCreateCorrectResultSource))]
    public void FailureMethodsShouldCreateCorrectResult(
        (Func<string[], ResultFailure> factoryMethod, ResultType expectedType) tuple
    )
    {
        var failure = tuple.factoryMethod.Invoke(["test"]);
        ((Result)failure).ShouldBeFailed(tuple.expectedType, "test");
    }

    private static IEnumerable<(
        Func<string[], ResultFailure> factoryMethod,
        ResultType expectedType
    )> FailureMethodsShouldCreateCorrectResultSource()
    {
        yield return (Result.NotFound, ResultType.NotFound);
        yield return (Result.Conflict, ResultType.Conflict);
        yield return (Result.ExternalServiceError, ResultType.ExternalServiceError);
        yield return (Result.RuleViolation, ResultType.RuleViolation);
        yield return (Result.InternalError, ResultType.InternalError);
        yield return (Result.PaymentRequired, ResultType.PaymentRequired);
        yield return (Result.Unauthorized, ResultType.Unauthorized);
        yield return (Result.Forbidden, ResultType.Forbidden);
        yield return (Result.Canceled, ResultType.Canceled);
        yield return (Result.Timeout, ResultType.Timeout);
    }

    [Test]
    public void ValueGetterShouldThrowOnGenericResultIfSucceededIsFalse()
    {
        Result<string> GetFailed() => Result.RuleViolation([]);

        var result = GetFailed();
        Should.Throw<InvalidOperationException>(() => result.Value);
    }

    [Test]
    public void ValueGetterShouldNotThrowOnGenericResultIfSucceededIsTrue()
    {
        var result = Result.Success("test");

        Should.NotThrow(() => result.Value);
    }

    [Test]
    public void ImplicitOperatorShouldWorkOnResult()
    {
        ResultFailure failure = Result.NotFound();
        Result result = failure;
        result.ShouldBeFailed(ResultType.NotFound);
    }

    [Test]
    public void ImplicitOperatorShouldWorkOnGenericResult()
    {
        ResultFailure failure = Result.NotFound();
        Result<string> result = failure;
        result.ShouldBeFailed(ResultType.NotFound);
    }

    [Test]
    public void ToFailureShouldThrowIfResultSucceeded()
    {
        var result = Result.Success("test");

        Should.Throw<InvalidOperationException>(result.ToFailure<int>);
    }

    [Test]
    public void ToFailureShouldChangeGenericType()
    {
        Result<string> GetStringFailure() => Result.NotFound();
        Result<int> GetIntFailure() => Result.NotFound();

        var failure = GetStringFailure();

        var newFailure = failure.ToFailure<int>();
        var type = newFailure.GetType();
        type.ShouldBe(GetIntFailure().GetType());
    }

    [Test]
    public void ResultFailureConstructorShouldThrowIfSuccessTypePassed()
    {
        Should.Throw<InvalidOperationException>(() => new ResultFailure(ResultType.Success));
    }

    [Test]
    public void ResultFailureConstructorShouldCreateResultFailure()
    {
        var failure = new ResultFailure(ResultType.NotFound, "test");
        failure.Type.ShouldBe(ResultType.NotFound);
        failure.Errors.ShouldBe(["test"]);
    }
}
