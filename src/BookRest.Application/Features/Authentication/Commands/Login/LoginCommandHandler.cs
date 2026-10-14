using BookRest.Application.Common.Interfaces;
using BookRest.Application.Common.Interfaces.Repositories;
using BookRest.Application.Features.Authentication.Common;
using BookRest.Domain.Entities;

namespace BookRest.Application.Features.Authentication.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthenticationResponseDto>
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

    public async Task<AuthenticationResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var userInfo = await _identityService.GetByEmailAsync(request.Email);

        if (userInfo == null || !await _identityService.CheckPasswordAsync(userInfo.ApplicationUserId, request.Password))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        // Currently use roles, then switch to Permission Based Authorization
        var roles = await _identityService.GetRolesAsync(userInfo.ApplicationUserId);

        // Generate tokens
        var (accessTokenValue, accessTokenExpiresAt) = _tokenProvider.GenerateAccessToken(userInfo, roles);
        var (refreshTokenValue, refreshTokenExpiresAt) = _tokenProvider.GenerateRefreshToken();

        // Store refresh token in the database
        var refreshToken = RefreshToken.Create(refreshTokenValue, refreshTokenExpiresAt, userInfo.ApplicationUserId);

        _refreshTokenRepository.Add(refreshToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthenticationResponseDto(
            accessTokenValue,
            refreshTokenValue,
            accessTokenExpiresAt.ToString(),
            refreshTokenExpiresAt.ToString(),
            userInfo.ApplicationUserId);
    }
}
