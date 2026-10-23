using BookRest.Application.Common.Interfaces;
using BookRest.Application.Common.Interfaces.Messaging;
using BookRest.Application.Common.Interfaces.Repositories;
using BookRest.Application.Features.Authentication.Common;
using BookRest.Domain.Entities;
using BookRest.Domain.Errors;
using BookRest.Domain.Shared;
using BookRest.Domain.ValueObjects;

namespace BookRest.Application.Features.Authentication.Commands.RegisterCustomer;

public class RegisterCustomerCommandHandler : ICommandHandler<RegisterCustomerCommand, AuthenticationResponseDto>
{
    private readonly IIdentityService _identityService;
    private readonly ICustomerRepository _customerRepository;
    private readonly ITokenProvider _tokenProvider;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCustomerCommandHandler(
        IIdentityService identityService,
        ICustomerRepository customerRepository,
        ITokenProvider tokenProvider,
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork)
    {
        _identityService = identityService;
        _customerRepository = customerRepository;
        _tokenProvider = tokenProvider;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthenticationResponseDto>> Handle(RegisterCustomerCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);

        if (await _customerRepository.ExistsByEmailAsync(email, cancellationToken))
        {
            return Result<AuthenticationResponseDto>.Failure(CustomerErrors.AlreadyExists(email.Value));
        }

        var result = await _identityService.CreateUserAsync(request.UserName, request.Email, request.Password);

        if (result.IsFailure)
        {
            if (result is IValidationResult validationResult)
            {
                return ValidationResult<AuthenticationResponseDto>.WithErrors(validationResult.Errors);
            }

            return Result<AuthenticationResponseDto>.Failure(result.Error!.Value);
        }

        var customer = Customer.Create(result.Value, request.UserName, email);

        _customerRepository.Add(customer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Auto login
        // Currently use roles, then switch to Permission Based Authorization
        var roles = await _identityService.GetRolesAsync(customer.ApplicationUserId);

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
