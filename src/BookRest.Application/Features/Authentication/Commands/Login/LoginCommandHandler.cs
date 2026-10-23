using BookRest.Application.Common.Errors;
using BookRest.Application.Common.Interfaces;
using BookRest.Application.Common.Interfaces.Messaging;
using BookRest.Application.Common.Interfaces.Repositories;
using BookRest.Application.Features.Authentication.Common;
using BookRest.Domain.Entities;
using BookRest.Domain.Shared;

namespace BookRest.Application.Features.Authentication.Commands.Login;

public class LoginCommandHandler : ICommandHandler<LoginCommand, AuthenticationResponseDto>
{
    private readonly IIdentityService _identityService;
    private readonly ITokenProvider _tokenProvider;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(
        IIdentityService identityService,
        ITokenProvider tokenProvider,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork)
    {
        _identityService = identityService;
        _tokenProvider = tokenProvider;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthenticationResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var userInfo = await _identityService.GetByEmailAsync(request.Email);

        if (userInfo == null || !await _identityService.CheckPasswordAsync(userInfo.ApplicationUserId, request.Password))
        {
            return Result<AuthenticationResponseDto>.Failure(IdentityErrors.InvalidCredentials());
        }

        // Currently use roles, then switch to Permission Based Authorization
        var roles = await _identityService.GetRolesAsync(userInfo.ApplicationUserId);

        // Generate tokens
        var (accessTokenValue, accessTokenExpiresAt) = _tokenProvider.GenerateAccessToken(userInfo, roles);
        var (refreshTokenValue, refreshTokenExpiresAt) = _tokenProvider.GenerateRefreshToken();

        var refreshToken = RefreshToken.Create(refreshTokenValue, refreshTokenExpiresAt, userInfo.ApplicationUserId);

        // Store refresh token in the database
        _refreshTokenRepository.Add(refreshToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var authenticationResponseDto = new AuthenticationResponseDto(
            accessTokenValue,
            refreshTokenValue,
            accessTokenExpiresAt.ToString(),
            refreshTokenExpiresAt.ToString(),
            userInfo.ApplicationUserId);

        return Result<AuthenticationResponseDto>.Success(authenticationResponseDto);
    }
}
