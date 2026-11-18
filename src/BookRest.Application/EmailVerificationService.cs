using BookRest.Application.Common.Interfaces;
using BookRest.Application.Common.Interfaces.Emailing;
using BookRest.Domain.Shared;

namespace BookRest.Application;

public class EmailVerificationService : IEmailVerificationService
{
    private readonly IIdentityService _identityService;
    private readonly IEmailVerificationLinkFactory _linkFactory;
    private readonly IEmailService _emailService;

    public EmailVerificationService(
        IIdentityService identityService,
        IEmailVerificationLinkFactory linkFactory,
        IEmailService emailService)
    {
        _identityService = identityService;
        _linkFactory = linkFactory;
        _emailService = emailService;
    }

    public async Task<Result> SendAsync(Guid applicationUserId, string toAddress, CancellationToken cancellationToken = default)
    {
        var tokenResult = await _identityService.GenerateEmailConfirmationTokenAsync(applicationUserId);

        if (tokenResult.IsFailure)
        {
            return Result.Failure(tokenResult.Error!.Value);
        }

        var verificationLink = _linkFactory.Create(applicationUserId, tokenResult.Value);

        return await _emailService.SendEmailAsync(
            toAddress: toAddress,
            subject: "Verify your email address",
            body: $"To verify your email address, open this link:\n{verificationLink}.",
            cancellationToken: cancellationToken);
    }
}
