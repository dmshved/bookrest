namespace BookRest.Application.Features.Authentication.Commands.VerifyEmail;

public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(c => c.ApplicationUserId)
            .NotEmpty()
            .NotEmpty().WithMessage("User Id is required.");

        RuleFor(c => c.EmailVerificationToken)
            .NotEmpty()
            .NotEmpty().WithMessage("Email confirmation token is required.");
    }
}
