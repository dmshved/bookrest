using BookRest.Domain.Errors;

namespace BookRest.Domain.Entities;

public sealed class RefreshToken : BaseAuditableEntity
{
    public string Token { get; private set; } = string.Empty;
    public Instant ExpiresAt { get; private set; }
    public Instant? RevokedAt { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public string? ReasonRevoked { get; private set; }
    public bool IsRevoked => RevokedAt != null;

    public Guid ApplicationUserId { get; set; }

    private RefreshToken() { }

    public static RefreshToken Create(
        string token,
        Instant expiresAt,
        Guid applicationUserId)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = token,
            ExpiresAt = expiresAt,
            ApplicationUserId = applicationUserId
        };
    }

    public Result Revoke(Instant revokedAt, string? reason = null, string? replacedByToken = null)
    {
        if (IsRevoked)
        {
            return Result.Failure(RefreshTokenErrors.AlreadyRevoked());
        }

        RevokedAt = revokedAt;
        ReplacedByToken = replacedByToken;
        ReasonRevoked = reason;

        return Result.Success();
    }

    public bool IsExpired(Instant now)
    {
        return now >= ExpiresAt;
    }

    public bool IsActive(Instant now)
    {
        return !IsRevoked && !IsExpired(now);
    }
}
