using BookRest.Domain.Entities;
using BookRest.Domain.Errors;
using NodaTime;

namespace BookRest.Domain.UnitTests.Entities;

public sealed class RefreshTokenTests
{
    private static readonly Instant Now = Instant.FromUtc(2026, 1, 1, 12, 0);
    private static readonly Instant ExpiresAt = Now.Plus(Duration.FromDays(7));

    private static RefreshToken CreateValidRefreshToken() => RefreshToken.Create(
        token: "token-value",
        expiresAt: ExpiresAt,
        applicationUserId: Guid.NewGuid());

    [Fact]
    public static void Create_WhenValidInput_ShouldReturnValidRefreshToken()
    {
        var applicationUserId = Guid.NewGuid();
        var refreshToken = RefreshToken.Create("token-value", ExpiresAt, applicationUserId);

        Assert.NotEqual(refreshToken.Id, Guid.Empty);
        Assert.Equal(refreshToken.Token, "token-value");
        Assert.Equal(refreshToken.ExpiresAt, ExpiresAt);
        Assert.Equal(refreshToken.ApplicationUserId, applicationUserId);
        Assert.False(refreshToken.IsRevoked);
    }

    [Fact]
    public static void Revoke_WhenNotRevoked_ShouldRevokeRefreshToken()
    {
        var refreshToken = CreateValidRefreshToken();

        refreshToken.Revoke(Now, "reason", "token-value");

        Assert.True(refreshToken.IsRevoked);
        Assert.Equal(refreshToken.ReasonRevoked, "reason");
        Assert.Equal(refreshToken.ReplacedByToken, "token-value");
    }

    [Fact]
    public static void Revoke_WhenRevoked_ShouldReturnFailure()
    {
        var refreshToken = CreateValidRefreshToken();
        refreshToken.Revoke(Now, "reason", "token-value");

        // Revoke again
        var result = refreshToken.Revoke(Now, "reason", "token-value");

        Assert.True(result.IsFailure);
        Assert.Equal(result.Error, RefreshTokenErrors.AlreadyRevoked());
    }

    [Fact]
    public static void IsExpired_WhenTimeApproachesExpiration_ShouldBecomeExpired()
    {
        var refreshToken = CreateValidRefreshToken();

        // Fresh refresh token
        Assert.False(refreshToken.IsExpired(Now));
        // Just before being expired
        Assert.False(refreshToken.IsExpired(ExpiresAt - Duration.FromSeconds(1)));
        // Expired
        Assert.True(refreshToken.IsExpired(ExpiresAt));
        // Just after being expired
        Assert.True(refreshToken.IsExpired(ExpiresAt + Duration.FromSeconds(1)));
    }

    [Fact]
    public static void IsActive_WhenIsNotExpiredAndIsNotRevoked_ShouldBeActive()
    {
        var refreshToken = CreateValidRefreshToken();

        Assert.True(refreshToken.IsActive(Now));
    }

    [Fact]
    public static void IsActive_WhenExpired_ShoulNotBeActive()
    {
        var refreshToken = CreateValidRefreshToken();

        Assert.False(refreshToken.IsActive(ExpiresAt));
    }

    [Fact]
    public static void IsActive_WhenRevoked_ShoulNotBeActive()
    {
        var refreshToken = CreateValidRefreshToken();

        refreshToken.Revoke(Now);

        Assert.False(refreshToken.IsActive(Now));
    }
}
