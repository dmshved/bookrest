namespace BookRest.Domain.Shared;

/// <summary>
/// Represents a generic type for explicit error handling.
/// </summary>
public readonly record struct Result<T>
{
    private readonly T? _value;
    private readonly Error? _error;
    private readonly bool _isSuccess;

    public bool IsSuccess => _isSuccess;
    public bool IsFailure => !_isSuccess;

    /// <summary>
    /// Represents success value. Throws InvalidOperationException if accessed on a failure result.
    /// </summary>
    public T Value => _isSuccess
        ? _value!
        : throw new InvalidOperationException($"Cannot access {nameof(Value)} on a failed result. Error: {_error?.Message}");

    /// <summary>
    /// Represents error details. Null on success.
    /// </summary>
    public Error? Error => _error;

    /// <summary>
    /// Initialises result type for success.
    /// </summary>
    private Result(T value)
    {
        _value = value;
        _error = null;
        _isSuccess = true;
    }

    /// <summary>
    /// Initialises result type for error.
    /// </summary>
    private Result(Error error)
    {
        _value = default;
        _error = error;
        _isSuccess = false;
    }

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(Error error) => new(error);

    public static Result<T> Failure(string code, string message) => new(new Error(code, message));
}

public readonly record struct Result
{
    private readonly Error? _error;
    private readonly bool _isSuccess;

    public bool IsSuccess => _isSuccess;
    public bool IsFailure => !_isSuccess;

    public Error? Error => _error;

    private Result(Error? error)
    {
        _error = error;
        _isSuccess = error is null;
    }

    public static Result Success() => new(null);

    public static Result Failure(Error error) => new(error);

    public static Result Failure(string code, string message) => new(new Error(code, message));
}

