using BookRest.Application.Common.Interfaces;
using BookRest.Application.Common.Interfaces.Emailing;
using BookRest.Domain.Shared;

namespace BookRest.Application;

public class PasswordResetService : IPasswordResetService
{
    private readonly IIdentityService _identityService;
    private readonly IPasswordResetLinkFactory _linkFactory;
    private readonly IEmailService _emailService;

    public PasswordResetService(
        IIdentityService identityService,
        IPasswordResetLinkFactory linkFactory,
        IEmailService emailService)
    {
        _identityService = identityService;
        _linkFactory = linkFactory;
        _emailService = emailService;
    }

    public async Task<Result> SendAsync(Guid applicationUserId, string toAddress, CancellationToken cancellationToken = default)
    {
        var tokenResult = await _identityService.GeneratePasswordResetTokenAsync(applicationUserId);

        if (tokenResult.IsFailure)
        {
            return Result.Failure(tokenResult.Error!.Value);
        }

        var verificationLink = _linkFactory.Create(applicationUserId, tokenResult.Value);
        // var encodedVerificationLink = WebUtility.HtmlEncode(verificationLink);

        return await _emailService.SendEmailAsync(
            toAddress: toAddress,
            subject: "Reset your password",
            body: $"<p>To reset your password, open this link:</p>" +
                  $"<p><a href='{verificationLink}'>{verificationLink}</a></p>" +
                  $"<p>If you didn't request this, ignore this email.</p>",
            cancellationToken: cancellationToken);
    }
}
