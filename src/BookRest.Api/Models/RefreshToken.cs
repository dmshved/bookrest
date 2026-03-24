namespace BookRest.Api.Models;

internal class RefreshToken
{
    public Guid Id { get; set; }
    public required string Token { get; set; }
    public required string UserId { get; set; }
    public DateTime ExpiresOnUtc { get; set; }
    public DateTime CreatedOnUtc { get; set; }

    public User User { get; set; }
}
