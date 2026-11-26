namespace BookRest.Application.Features.Customers.Commands.UpdateCustomerProfile;

public class UpdateCustomerProfileCommandValidator : AbstractValidator<UpdateCustomerProfileCommand>
{
    public UpdateCustomerProfileCommandValidator()
    {
        RuleFor(c => c.UserName)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(4).WithMessage("Username must be at least 4 characters.")
            .MaximumLength(50).WithMessage("Username must be 50 characters or less.");
    }
}
