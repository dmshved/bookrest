namespace BookRest.Domain.Errors;

public static class RefreshTokenErrors
{
    public static Error AlreadyRevoked() =>
        new("RefreshToken.AlreadyRevoked",
            "Token is already revoked.",
            ErrorType.Conflict);
}
