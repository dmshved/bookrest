namespace BookRest.Domain.Shared;

/// <summary>
/// Represents error types.
/// </summary>
public enum ErrorType
{
    Failure,
    NotFound,
    Validation,
    Conflict,
    Unauthorized,
    Forbidden
}
