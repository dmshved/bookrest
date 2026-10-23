using BookRest.Domain.Shared;

namespace BookRest.Application.Common.Errors;

public static class IdentityErrors
{
    public static Error InvalidCredentials() =>
        new("Login.InvalidCredentials",
            "Invalid email or password.",
            ErrorType.Unauthorized);
}
