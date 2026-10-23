namespace BookRest.Domain.Shared;

/// <summary>
/// Represents a generic validation result type for explicit validation error handling.
/// </summary>
public class ValidationResult<T> : Result<T>, IValidationResult
{
    private readonly Error[] _errors;

    public Error[] Errors => _errors;

    /// <summary>
    /// Initialises a successful generic validation result.
    /// </summary>
    private ValidationResult(T value)
        : base(value)
    {
        _errors = [];
    }

    /// <summary>
    /// Initialises a failed generic validation result.
    /// </summary>
    private ValidationResult(Error[] errors)
        : base(IValidationResult.ValidationError)
    {
        _errors = errors;
    }

    public new static ValidationResult<T> Success(T value) => new(value);

    public static ValidationResult<T> WithErrors(Error[] errors) => new(errors);
}

/// <summary>
/// Represents a non-generic validation result type for explicit validation error handling.
/// </summary>
public class ValidationResult : Result, IValidationResult
{
    private readonly Error[] _errors;

    public Error[] Errors => _errors;

    /// <summary>
    /// Initialises a successful validation result.
    /// </summary>
    private ValidationResult()
    {
        _errors = [];
    }

    /// <summary>
    /// Initialises a failed validation result.
    /// </summary>
    private ValidationResult(Error[] errors)
        : base(IValidationResult.ValidationError)
    {
        _errors = errors;
    }

    public new static ValidationResult Success() => new();

    public static ValidationResult WithErrors(Error[] errors) => new(errors);
}
