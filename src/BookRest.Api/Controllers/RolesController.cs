using BookRest.Api.Abstractions;
using BookRest.Application.Features.Permisions.Commands.GrantPermission;
using BookRest.Application.Features.Permisions.Commands.RevokePermission;
using BookRest.Application.Features.Permisions.Common;
using BookRest.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookRest.Api.Controllers;

[Route("api/v1/admin/[controller]")]
[Authorize(Policy = Permissions.Roles.Manage)]
public class RolesController : ApiController
{
    public RolesController(ISender sender) : base(sender)
    {
    }

    [HttpPost("{roleName}/permissions")]
    public async Task<IActionResult> Grant(string roleName, [FromBody] GrantPermissionRequestDto request)
    {
        var command = new GrantPermissionCommand(roleName, request.PermissionName);
        var result = await Sender.Send(command);

        return ToHttpResult(result);
    }

    [HttpDelete("{roleName}/permissions")]
    public async Task<IActionResult> Revoke(string roleName, [FromBody] RevokePermissionRequestDto request)
    {
        var command = new RevokePermissionCommand(roleName, request.PermissionName);
        var result = await Sender.Send(command);

        return ToHttpResult(result);
    }
}
