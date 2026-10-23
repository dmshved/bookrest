namespace BookRest.Domain.Shared;

/// <summary>
/// Represents a non-generic result type for explicit error handling.
/// </summary>
public class Result
{
    private readonly Error? _error;
    private readonly bool _isSuccess;

    public bool IsSuccess => _isSuccess;
    public bool IsFailure => !_isSuccess;

    public Error? Error => _error;

    /// <summary>
    /// Initialises a successful result.
    /// </summary>
    protected Result()
    {
        _error = null;
        _isSuccess = true;
    }

    /// <summary>
    /// Initialises a failed result.
    /// </summary>
    protected Result(Error error)
    {
        _error = error;
        _isSuccess = false;
    }

    public static Result Success() => new();

    public static Result Failure(Error error) => new(error);

    public static Result Failure(string code, string message) => new(new Error(code, message));
}

/// <summary>
/// Represents a generic result type for explicit error handling.
/// </summary>
public class Result<T> : Result
{
    private readonly T? _value;

    /// <summary>
    /// Represents success value. Throws InvalidOperationException if accessed on a failure result.
    /// </summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value on a failed result");

    /// <summary>
    /// Initialises generic result type for success.
    /// </summary>
    protected Result(T value)
        : base()
    {
        _value = value;
    }

    /// <summary>
    /// Initialises generic result type for error.
    /// </summary>
    protected Result(Error error)
        : base(error)
    {
        _value = default;
    }

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(Error error) => new(error);

    public static Result<T> Failure(string code, string message) => new(new Error(code, message));
}
