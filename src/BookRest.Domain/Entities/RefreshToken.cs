namespace BookRest.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public required string Token { get; set; }
    public required string UserId { get; set; }
    public DateTime ExpiresOnUtc { get; set; }
    public DateTime CreatedOnUtc { get; set; }
}