using System.Text;

namespace VsaTemplate.Common.Models;

public record Result
{
    protected Result(bool succeeded, IEnumerable<string> errors, ResultType type)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
        Type = type;
    }

    public bool Succeeded { get; }
    public IReadOnlyList<string> Errors { get; }
    public ResultType Type { get; }

    public static Result Success()
    {
        return new Result(true, [], ResultType.Success);
    }

    public static Result<T> Success<T>(T value)
    {
        return Result<T>.Success(value);
    }

    public static ResultFailure NotFound(params string[] errors) =>
        new(ResultType.NotFound, errors);

    public static ResultFailure Conflict(params string[] errors) =>
        new(ResultType.Conflict, errors);

    public static ResultFailure ExternalServiceError(params string[] errors) =>
        new(ResultType.ExternalServiceError, errors);

    public static ResultFailure RuleViolation(params string[] errors) =>
        new(ResultType.RuleViolation, errors);

    public static ResultFailure InternalError(params string[] errors) =>
        new(ResultType.InternalError, errors);

    public static ResultFailure PaymentRequired(params string[] errors) =>
        new(ResultType.PaymentRequired, errors);

    public static ResultFailure Unauthorized(params string[] errors) =>
        new(ResultType.Unauthorized, errors);

    public static ResultFailure Forbidden(params string[] errors) =>
        new(ResultType.Forbidden, errors);

    public static ResultFailure Canceled(params string[] errors) =>
        new(ResultType.Canceled, errors);

    public static ResultFailure Timeout(params string[] errors) => new(ResultType.Timeout, errors);

    public static implicit operator Result(ResultFailure failure)
    {
        return new Result(false, failure.Errors, failure.Type);
    }
}

public sealed record Result<T> : Result
{
    private Result(bool succeeded, string[] errors, ResultType type, T value)
        : base(succeeded, errors, type)
    {
        Value = value;
    }

    public T Value
    {
        get
        {
            if (!Succeeded)
            {
                throw new InvalidOperationException("Failed result does not have inner value.");
            }

            return field;
        }
    }

    public static implicit operator Result<T>(ResultFailure failure)
    {
        return new Result<T>(false, failure.Errors, failure.Type, default!);
    }

    public static Result<T> Success(T value)
    {
        return new Result<T>(true, [], ResultType.Success, value);
    }

    public Result<TOther> ToFailure<TOther>()
    {
        if (Succeeded)
        {
            throw new InvalidOperationException(
                "Cannot convert to new Result failure when Result is succeeded."
            );
        }

        return new Result<TOther>(
            succeeded: false,
            errors: Errors.ToArray(),
            value: default!,
            type: Type
        );
    }

    // This is required so the getter on Value does not throw if Succeeded is false
    protected override bool PrintMembers(StringBuilder builder)
    {
        builder.Append("Succeeded = ").Append(Succeeded).Append(", Type = ").Append(Type);

        builder.Append(", Value = ");

        if (Succeeded)
            builder.Append(Value);

        builder.Append(", Errors = ").Append(string.Join(", ", Errors));

        return true;
    }
}

public class ResultFailure
{
    public ResultType Type { get; }
    public string[] Errors { get; }

    public ResultFailure(ResultType type, params string[] errors)
    {
        if (type == ResultType.Success)
            throw new InvalidOperationException($"'{type}' is not a failure type.");

        Type = type;
        Errors = errors.ToArray();
    }
}

public enum ResultType
{
    Success, //Ok, NoContent

    Canceled, // Client closed connection
    Timeout, // Gateway Timeout - can be named to ExternalServiceTimeout and also add another member: Request Timeout
    NotFound,
    Conflict,
    ExternalServiceError, //TypedResults.Problem
    RuleViolation, //BadRequest
    InternalError, //internal server error
    PaymentRequired,
    Unauthorized,
    Forbidden,
}
