namespace Backend.Api.Core.Common;

public sealed record Error(string Code, string Message)
{
    public string Type => Code;
}

public enum ResultError
{
    Validation,
    NotFound,
    Forbidden,
    Conflict,
    Internal
}

public class Result
{
    public bool IsSuccess { get; }
    public Error? Error { get; }

    protected Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() =>
        new(true, null);

    public static Result Failure(Error error) =>
        new(false, error);

    public static Result<T> Success<T>(T value) =>
        Result<T>.CreateSuccess(value);

    // TODO: Remove unnesscessary methods...
    public static Result<T> Failure<T>(ResultError type, string message) =>
        Result<T>.CreateFailure(new Error(type.ToString().ToLowerInvariant(), message));

    public static Result<T> Failure<T>(ResultError type) =>
        Result<T>.CreateFailure(new Error(type.ToString().ToLowerInvariant(), ""));

    public static Result<T> Failure<T>(string code, string message) =>
        Result<T>.CreateFailure(new Error(code, message));
    // ... Up to here (After implementing proper error codes instead of using ResultError)

    public static Result<T> Failure<T>(Error error) =>
        Result<T>.CreateFailure(error);

    // public static Result Failure<T>(
    //     ResultError type, string message) =>
    //     Result<T>.CreateFailure(
    //         [new Error(type.ToString().ToLowerInvariant(), message)]);
}

public sealed class Result<T> : Result
{
    public T? Value { get; }

    private Result(T value)
        : base(true, null)
    {
        Value = value;
    }

    private Result(Error error)
        : base(false, error)
    { }

    internal static Result<T> CreateSuccess(T value) =>
        new(value);

    internal static Result<T> CreateFailure(Error error) =>
        new(error);

    public static Result<T> Success(T value) => new(value);

    public static new Result<T> Failure(Error error) =>
        new(error);

    public static Result<T> Failure(ResultError type, string message) =>
        Failure(new Error(type.ToString().ToLowerInvariant(), message));
}
