using BookRest.Application.Common.Interfaces;
using BookRest.Application.Common.Interfaces.Messaging;
using BookRest.Application.Common.Interfaces.Repositories;
using BookRest.Application.Features.Authentication.Common;
using BookRest.Domain.Constants;
using BookRest.Domain.Entities;
using BookRest.Domain.Errors;
using BookRest.Domain.Shared;
using BookRest.Domain.ValueObjects;

namespace BookRest.Application.Features.Authentication.Commands.RegisterOwner;

public class RegisterOwnerCommandHandler : ICommandHandler<RegisterOwnerCommand, AuthenticationResponseDto>
{
    private readonly IIdentityService _identityService;
    private readonly IOwnerRepository _ownerRepository;
    private readonly ITokenProvider _tokenProvider;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterOwnerCommandHandler(
        IIdentityService identityService,
        IOwnerRepository ownerRepository,
        ITokenProvider tokenProvider,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork)
    {
        _identityService = identityService;
        _ownerRepository = ownerRepository;
        _tokenProvider = tokenProvider;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthenticationResponseDto>> Handle(RegisterOwnerCommand request, CancellationToken cancellationToken)
    {
        var emailResult = Email.Create(request.Email);

        if (emailResult.IsFailure)
        {
            return Result<AuthenticationResponseDto>.Failure(emailResult.Error!.Value);
        }

        var email = emailResult.Value;

        if (await _ownerRepository.ExistsByEmailAsync(email, cancellationToken))
        {
            return Result<AuthenticationResponseDto>.Failure(OwnerErrors.AlreadyExists(email.Value));
        }

        var userResult = await _identityService.CreateUserAsync(request.UserName, request.Email, request.Password);

        if (userResult.IsFailure)
        {
            if (userResult is IValidationResult validationResult)
            {
                return ValidationResult<AuthenticationResponseDto>.WithErrors(validationResult.Errors);
            }

            return Result<AuthenticationResponseDto>.Failure(userResult.Error!.Value);
        }

        var roleResult = await _identityService.AddToRoleAsync(userResult.Value, Roles.Owner);

        if (roleResult.IsFailure)
        {
            await _identityService.DeleteUserAsync(userResult.Value);
            return Result<AuthenticationResponseDto>.Failure(roleResult.Error!.Value);
        }

        var owner = Owner.Create(userResult.Value, request.UserName, email);

        _ownerRepository.Add(owner);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var roles = await _identityService.GetRolesAsync(owner.ApplicationUserId);

        var userInfo = await _identityService.GetByEmailAsync(email.Value);

        // Generate tokens
        var (accessTokenValue, accessTokenExpiresAt) = _tokenProvider.GenerateAccessToken(userInfo!, roles);
        var (refreshTokenValue, refreshTokenExpiresAt) = _tokenProvider.GenerateRefreshToken();

        var refreshToken = RefreshToken.Create(refreshTokenValue, refreshTokenExpiresAt, userInfo!.ApplicationUserId);

        // Store refresh token in the database
        _refreshTokenRepository.Add(refreshToken);
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
