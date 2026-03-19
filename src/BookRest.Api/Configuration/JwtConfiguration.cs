namespace BookRest.Api.Configuration;

public record class JwtConfiguration
{
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required double ExpirationInMinutes { get; init; }
    public required string SecretKey { get; init; }
}