namespace BookRest.Application.Features.Authentication.Commands.Revoke;

public class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommand>
{
    public RevokeTokenCommandValidator()
    {
        RuleFor(c => c.RefreshToken)
            .NotEmpty().WithMessage("Refresh token cannot be empty.");
    }
}
