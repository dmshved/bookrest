using System.Security.Claims;
using BookRest.Application.Common.Interfaces;
using BookRest.Domain.Constants;
using BookRest.Domain.ValueObjects;
using BookRest.Infrastructure.Data.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace BookRest.Infrastructure.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IOptions<PermissionOptions> _permissionOptions;
    // Switch to the PermissionStore cache service later
    private readonly IPermissionService _permissionService;

    public PermissionAuthorizationHandler(
        IOptions<PermissionOptions> permissionOptions,
        IPermissionService permissionService)
    {
        _permissionOptions = permissionOptions;
        _permissionService = permissionService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var permissionOptions = _permissionOptions.Value;

        if (context.User.Identity?.IsAuthenticated is not true)
        {
            return;
        }

        IReadOnlyCollection<string> permissions;

        // Read permissions from the token
        if (permissionOptions.PermissionSource == PermissionSource.Token)
        {
            permissions = context.User.FindAll(Permissions.ClaimType)
                .Select(c => c.Value)
                .ToArray();
        }
        // Read permissions from the database
        else
        {
            var id = context.User.FindFirstValue("application_user_id");

            if (!Guid.TryParse(id, out var applicationUserId))
            {
                return;
            }

            permissions = await _permissionService.GetPermissionsAsync(applicationUserId);
        }

        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}
