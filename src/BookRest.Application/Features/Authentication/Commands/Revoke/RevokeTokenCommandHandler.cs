using BookRest.Application.Common.Interfaces;
using BookRest.Application.Common.Interfaces.Messaging;
using BookRest.Application.Common.Interfaces.Repositories;
using BookRest.Domain.Shared;

namespace BookRest.Application.Features.Authentication.Commands.Revoke;

public class RevokeTokenCommandHandler : ICommandHandler<RevokeTokenCommand>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IIdentityService _identityService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public RevokeTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IIdentityService identityService,
        IUnitOfWork unitOfWork,
        IClock clock)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _identityService = identityService;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
    {
        var storedRefreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken, cancellationToken);

        if (storedRefreshToken == null)
        {
            return Result.Success();
        }

        var userInfo = await _identityService.GetByIdAsync(storedRefreshToken.ApplicationUserId);

        if (userInfo == null)
        {
            return Result.Success();
        }

        var now = _clock.GetCurrentInstant();

        storedRefreshToken.Revoke(
           revokedAt: now,
           reason: request.Reason ?? "User logout.");

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
