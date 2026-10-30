using BookRest.Application.Common.Interfaces;
using BookRest.Application.Common.Interfaces.Messaging;
using BookRest.Domain.Shared;

namespace BookRest.Application.Features.Permisions.Commands.GrantPermission;

public class GrantPermissionCommandHandler : ICommandHandler<GrantPermissionCommand>
{
    private readonly IPermissionService _permissionService;

    public GrantPermissionCommandHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public Task<Result> Handle(GrantPermissionCommand request, CancellationToken cancellationToken)
    {
        return _permissionService.GrantPermissionAsync(request.RoleName, request.PermissionName, cancellationToken);
    }
}
