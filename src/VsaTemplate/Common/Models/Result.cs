namespace VsaTemplate.Common.Models;

public readonly struct Result
{
    private Result(string[] errors, ResultType type)
    {
        Errors = errors;
        Type = type;
    }

    public bool Succeeded => Type == ResultType.Success;
    public string[] Errors { get; }
    public ResultType Type { get; }

    public static Result Success()
    {
        return new Result([], ResultType.Success);
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
        return new Result(failure.Errors, failure.Type);
    }
}

public readonly struct Result<T>
{
    private Result(string[] errors, ResultType type, T value)
    {
        Errors = errors;
        Type = type;
        Value = value;
    }

    public ResultType Type { get; }
    public string[] Errors { get; }
    public bool Succeeded => Type == ResultType.Success;

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
        return new Result<T>(failure.Errors, failure.Type, default!);
    }

    public static Result<T> Success(T value)
    {
        return new Result<T>([], ResultType.Success, value);
    }

    public Result<TOther> ToFailure<TOther>()
    {
        if (Succeeded)
        {
            throw new InvalidOperationException(
                "Cannot convert to new Result failure when Result is succeeded."
            );
        }

        return new Result<TOther>(errors: Errors, value: default!, type: Type);
    }
}

public readonly struct ResultFailure
{
    public ResultType Type { get; }
    public string[] Errors { get; }

    public ResultFailure(ResultType type, params string[] errors)
    {
        if (type == ResultType.Success)
            throw new InvalidOperationException($"'{type}' is not a failure type.");

        Type = type;
        Errors = errors;
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
