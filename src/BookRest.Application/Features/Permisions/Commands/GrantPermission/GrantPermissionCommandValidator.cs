using BookRest.Domain.Constants;

namespace BookRest.Application.Features.Permisions.Commands.GrantPermission;

public class GrantPermissionCommandValidator : AbstractValidator<GrantPermissionCommand>
{
    public GrantPermissionCommandValidator()
    {
        RuleFor(c => c.RoleName)
            .NotEmpty().WithMessage("Role name is required.");

        RuleFor(c => c.PermissionName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Permission is required.")
            .Must(permission => Permissions.All.Contains(permission)).WithMessage("Unknown permission.")
            .When(c => !string.IsNullOrWhiteSpace(c.PermissionName));
    }
}
