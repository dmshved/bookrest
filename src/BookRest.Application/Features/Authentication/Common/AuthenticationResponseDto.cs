namespace BookRest.Application.Features.Authentication.Common;

// public record AuthenticationResponseDto(
//     string AccessToken,
//     string RefreshToken,
//     Instant AccessTokenExpiresAt,
//     Instant RefreshTokenExpiresAt,
//     Guid ApplicationUserId);

public record AuthenticationResponseDto(
    string AccessToken,
    string RefreshToken,
    string AccessTokenExpiresAt,
    string RefreshTokenExpiresAt,
    Guid ApplicationUserId);
