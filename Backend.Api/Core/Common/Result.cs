namespace Backend.Api.Core.Common;

public sealed record Error(string Code, string Message);

public class Result
{
    public bool IsSuccess { get; }
    public IReadOnlyList<Error> Errors { get; }

    protected Result(bool isSuccess, IReadOnlyList<Error> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
    }

    public static Result Success() =>
        new(true, []);

    public static Result Failure(params Error[] errors) =>
        new(false, errors);
}

public sealed class Result<T> : Result
{
    public T? Value { get; }

    private Result(T value)
        : base(true, [])
    {
        Value = value;
    }

    private Result(IReadOnlyList<Error> errors)
        : base(false, errors)
    {
    }

    public static Result<T> Success(T value) => new(value);

    public static new Result<T> Failure(params Error[] errors) =>
        new(errors);
}
