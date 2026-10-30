using BookRest.Domain.Shared;

namespace BookRest.Application.Common.Errors;

/// <summary>
/// Represents application user errors.
/// </summary>
public static class IdentityErrors
{
    public static Error InvalidCredentials() =>
        new("Login.InvalidCredentials",
            "Invalid email or password.",
            ErrorType.Unauthorized);

    public static Error Unauthorized() =>
        new("Auth.Unauthorized",
            "Authorization required.",
            ErrorType.Unauthorized);

    public static Error NotFound(string applicationUserId) =>
        new("User.NotFound",
            $"User with id '{applicationUserId}' was not found.",
            ErrorType.NotFound);

    public static Error FailedAddToRole(string role) =>
        new("User.FailedAddToRole",
            $"Failed to add role '{role}' to the user.",
            ErrorType.Failure);
}
