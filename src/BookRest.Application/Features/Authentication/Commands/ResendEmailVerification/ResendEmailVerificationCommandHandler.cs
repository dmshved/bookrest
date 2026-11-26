using BookRest.Application.Common.Interfaces;
using BookRest.Application.Common.Interfaces.Emailing;
using BookRest.Domain.Shared;

namespace BookRest.Application.Features.Authentication.Commands.ResendEmailVerification;

public class ResendEmailVerificationCommandHandler : ICommandHandler<ResendEmailVerificationCommand>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailVerificationService _emailVerificationService;

    public ResendEmailVerificationCommandHandler(
        IIdentityService identityService,
        IEmailVerificationService emailVerificationService)
    {
        _identityService = identityService;
        _emailVerificationService = emailVerificationService;
    }

    public async Task<Result> Handle(ResendEmailVerificationCommand request, CancellationToken cancellationToken)
    {
        var userInfo = await _identityService.GetByEmailAsync(request.Email);

        if (userInfo == null || await _identityService.IsEmailConfirmedAsync(userInfo.ApplicationUserId))
        {
            return Result.Success();
        }

        await _emailVerificationService.SendAsync(userInfo.ApplicationUserId, userInfo.Email, cancellationToken);

        return Result.Success();
    }
}
