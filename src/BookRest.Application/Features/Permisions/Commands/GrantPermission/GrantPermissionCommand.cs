using BookRest.Application.Common.Interfaces.Messaging;

namespace BookRest.Application.Features.Permisions.Commands.GrantPermission;

public record GrantPermissionCommand(
    string RoleName,
    string PermissionName) : ICommand;
