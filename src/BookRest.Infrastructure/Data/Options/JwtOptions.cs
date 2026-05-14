namespace BookRest.Infrastructure.Data.Options;

public class JwtOptions
{
    public const string SectionName = "JwtOptions";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Expiration { get; set; } = string.Empty;
    public string SigningKey { get; set; } = string.Empty;
}