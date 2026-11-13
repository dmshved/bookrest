using Asp.Versioning;
using BookRest.Api.Abstractions;
using BookRest.Application.Features.Permisions.Commands.GrantPermission;
using BookRest.Application.Features.Permisions.Commands.RevokePermission;
using BookRest.Application.Features.Permisions.Common;
using BookRest.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookRest.Api.Controllers;

[ApiVersion(1)]
[Route("api/v{v:apiVersion}/admin/[controller]")]
[Authorize(Policy = Permissions.Roles.Manage)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class RolesController : ApiController
{
    public RolesController(ISender sender) : base(sender)
    {
    }

    /// <summary>
    /// Grant a permission.
    /// </summary>
    [MapToApiVersion(1)]
    [HttpPost("{roleName}/permissions")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Grant(string roleName, [FromBody] GrantPermissionRequestDto request)
    {
        var command = new GrantPermissionCommand(roleName, request.PermissionName);
        var result = await Sender.Send(command);

        return ToHttpResult(result);
    }

    /// <summary>
    /// Revoke a permission.
    /// </summary>
    [MapToApiVersion(1)]
    [HttpDelete("{roleName}/permissions")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Revoke(string roleName, [FromBody] RevokePermissionRequestDto request)
    {
        var command = new RevokePermissionCommand(roleName, request.PermissionName);
        var result = await Sender.Send(command);

        return ToHttpResult(result);
    }
}
