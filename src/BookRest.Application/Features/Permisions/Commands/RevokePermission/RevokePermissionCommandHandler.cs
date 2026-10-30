using BookRest.Application.Common.Interfaces;
using BookRest.Application.Common.Interfaces.Messaging;
using BookRest.Domain.Shared;

namespace BookRest.Application.Features.Permisions.Commands.RevokePermission;

public class RevokePermissionCommandHandler : ICommandHandler<RevokePermissionCommand>
{
    private readonly IPermissionService _permissionService;

    public RevokePermissionCommandHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public Task<Result> Handle(RevokePermissionCommand request, CancellationToken cancellationToken)
    {
        return _permissionService.RevokePermissionAsync(request.RoleName, request.PermissionName, cancellationToken);
    }
}
