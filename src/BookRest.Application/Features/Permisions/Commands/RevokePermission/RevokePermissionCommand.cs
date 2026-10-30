using BookRest.Application.Common.Interfaces.Messaging;

namespace BookRest.Application.Features.Permisions.Commands.RevokePermission;

public record RevokePermissionCommand(
    string RoleName,
    string PermissionName) : ICommand;
