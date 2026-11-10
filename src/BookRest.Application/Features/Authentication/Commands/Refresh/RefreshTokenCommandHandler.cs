using BookRest.Application.Common.Errors;
using BookRest.Application.Common.Interfaces;
using BookRest.Application.Common.Interfaces.Messaging;
using BookRest.Application.Common.Interfaces.Repositories;
using BookRest.Application.Features.Authentication.Common;
using BookRest.Domain.Entities;
using BookRest.Domain.Shared;

namespace BookRest.Application.Features.Authentication.Commands.Refresh;

public class RefreshTokenCommandHandler : ICommandHandler<RefreshTokenCommand, AuthenticationResponseDto>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenProvider _tokenProvider;
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        ITokenProvider tokenProvider,
        IIdentityService identityService,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _tokenProvider = tokenProvider;
        _identityService = identityService;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result<AuthenticationResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Find the refresh token
        var storedRefreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);

        // If refresh token is not found - reject with 401
        if (storedRefreshToken == null)
        {
            return Result<AuthenticationResponseDto>.Failure(IdentityErrors.Unauthorized());
        }

        var userInfo = await _identityService.GetByIdAsync(storedRefreshToken.ApplicationUserId);

        if (userInfo == null)
        {
            return Result<AuthenticationResponseDto>.Failure(IdentityErrors.Unauthorized());
        }

        // Token found but already revoked - revoke all of the user’s tokens
        if (storedRefreshToken.IsRevoked)
        {
            await _refreshTokenRepository.RevokeAllActiveTokensAsync(
                userInfo.ApplicationUserId,
                "Refresh token was already revoked or replaced",
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<AuthenticationResponseDto>.Failure(IdentityErrors.Unauthorized());
        }

        var now = _clock.GetCurrentInstant();

        // Token found but expired: reject
        if (storedRefreshToken.IsExpired(now))
        {
            return Result<AuthenticationResponseDto>.Failure(IdentityErrors.Unauthorized());
        }

        var roles = await _identityService.GetRolesAsync(userInfo.ApplicationUserId);

        // Token found and active: rotate
        // Generate new pair of tokens
        var (accessTokenValue, accessTokenExpiresAt) = _tokenProvider.GenerateAccessToken(userInfo, roles);
        var (refreshTokenValue, refreshTokenExpiresAt) = _tokenProvider.GenerateRefreshToken();

        // Create new refresh token instance
        var newRefreshToken = RefreshToken.Create(refreshTokenValue, refreshTokenExpiresAt, userInfo.ApplicationUserId);

        // Revoke the old token
        storedRefreshToken.Revoke(
            revokedAt: now,
            reason: "Replaced by new token",
            replacedByToken: newRefreshToken.Token);

        // Store new refresh token in the database
        _refreshTokenRepository.Add(newRefreshToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // return AuthenticationResponseDto
        var authenticationResponseDto = new AuthenticationResponseDto(
            accessTokenValue,
            refreshTokenValue,
            accessTokenExpiresAt.ToString(),
            refreshTokenExpiresAt.ToString(),
            userInfo.ApplicationUserId);

        return Result<AuthenticationResponseDto>.Success(authenticationResponseDto);
    }
}
