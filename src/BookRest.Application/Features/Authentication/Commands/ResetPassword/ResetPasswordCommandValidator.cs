namespace BookRest.Application.Features.Authentication.Commands.ResetPassword;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(c => c.ApplicationUserId)
            .NotEmpty()
            .NotEmpty().WithMessage("User Id is required.");

        RuleFor(c => c.ResetToken)
            .NotEmpty()
            .NotEmpty().WithMessage("Password reset token is required.");

        RuleFor(c => c.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .Equal(c => c.ConfirmPassword)
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");
    }
}
