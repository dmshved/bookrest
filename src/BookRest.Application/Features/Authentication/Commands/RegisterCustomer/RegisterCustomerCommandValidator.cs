namespace BookRest.Application.Features.Authentication.Commands.RegisterCustomer;

internal class RegisterCustomerCommandValidator : AbstractValidator<RegisterCustomerCommand>
{
    public RegisterCustomerCommandValidator()
    {
        RuleFor(c => c.UserName)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(4).WithMessage("Username must be at least 4 characters.")
            .MaximumLength(50).WithMessage("Username must be 50 characters or less.");

        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(c => c.Password)
            .NotEmpty().WithMessage("Password is required.")
            .Equal(c => c.ConfirmPassword)
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");
    }
}
