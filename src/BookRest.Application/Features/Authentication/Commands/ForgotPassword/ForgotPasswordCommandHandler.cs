using BookRest.Application.Common.Interfaces;
using BookRest.Application.Common.Interfaces.Emailing;
using BookRest.Application.Common.Interfaces.Messaging;
using BookRest.Domain.Shared;

namespace BookRest.Application.Features.Authentication.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : ICommandHandler<ForgotPasswordCommand>
{
    private readonly IIdentityService _identityService;
    private readonly IPasswordResetService _passwordResetService;

    public ForgotPasswordCommandHandler(
        IIdentityService identityService,
        IPasswordResetService passwordResetService)
    {
        _identityService = identityService;
        _passwordResetService = passwordResetService;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var userInfo = await _identityService.GetByEmailAsync(request.Email);

        if (userInfo == null)
        {
            return Result.Success();
        }

        await _passwordResetService.SendAsync(userInfo.ApplicationUserId, request.Email, cancellationToken);

        return Result.Success();
    }
}
