namespace BookRest.Api.Models;

public class RefreshToken
{
    public Guid Id { get; set; }
    public string Token { get; set; }
    public string UserId { get; set; }
    public DateTime ExpiresOnUtc { get; set; }
    public DateTime CreatedOnUtc { get; set; }

    public AppUser User { get; set; }
}
