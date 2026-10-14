namespace BookRest.Domain.ValueObjects;

public sealed record JwtToken(
    string AccessToken,
    string RefreshToken,
    Instant AccessTokenExpiresAt,
    Instant RefreshTokenExpiresAt);
