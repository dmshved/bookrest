namespace BookRest.Application.Features.Authentication.Commands.VerifyEmail;

public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(c => c.ApplicationUserId).NotEmpty();
        RuleFor(c => c.EmailVerificationToken).NotEmpty();
    }
}
