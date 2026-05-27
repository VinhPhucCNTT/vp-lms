namespace Backend.Core.Common;

public record MResult<T, TError>( // Method result
    bool IsSuccess,
    T? Value,
    TError? Error)
{
    public static MResult<T, TError> Success(T value)
        => new(true, value, default);

    public static MResult<T, TError> Failure(TError error)
        => new(false, default, error);
}
