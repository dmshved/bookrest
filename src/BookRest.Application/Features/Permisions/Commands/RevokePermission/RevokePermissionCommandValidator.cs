using BookRest.Domain.Constants;

namespace BookRest.Application.Features.Permisions.Commands.RevokePermission;

public class RevokePermissionCommandValidator : AbstractValidator<RevokePermissionCommand>
{
    public RevokePermissionCommandValidator()
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
