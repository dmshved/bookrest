using BookRest.Application.Common.Interfaces;
using BookRest.Application.Common.Interfaces.Messaging;
using BookRest.Domain.Shared;

namespace BookRest.Application.Features.Authentication.Commands.VerifyEmail;

public class VerifyEmailCommandHandler : ICommandHandler<VerifyEmailCommand>
{
    private readonly IIdentityService _identityService;

    public VerifyEmailCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<Result> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        return _identityService.ConfirmEmailAsync(request.ApplicationUserId, request.EmailVerificationToken);
    }
}
