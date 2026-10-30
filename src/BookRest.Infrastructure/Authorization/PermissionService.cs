using System.Security.Claims;
using BookRest.Application.Common.Interfaces;
using BookRest.Domain.Constants;
using BookRest.Domain.Errors;
using BookRest.Domain.Shared;
using BookRest.Infrastructure.Identity;
using BookRest.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;

namespace BookRest.Infrastructure.Authorization;

public class PermissionService : IPermissionService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public PermissionService(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IReadOnlyCollection<string>> GetPermissionsAsync(Guid applicationUserId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(applicationUserId.ToString());

        if (user == null)
        {
            return Array.Empty<string>();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var roleName in roles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);

            if (role == null)
            {
                continue;
            }

            var claims = await _roleManager.GetClaimsAsync(role);

            foreach (var claim in claims.Where(c => c.Type == Permissions.ClaimType))
            {
                permissions.Add(claim.Value);
            }
        }

        return permissions;
    }

    public async Task<Result> GrantPermissionAsync(string roleName, string permission, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByNameAsync(roleName);

        if (role == null)
        {
            return Result.Failure(RoleErrors.NotFound(roleName));
        }

        var claims = await _roleManager.GetClaimsAsync(role);

        var isGranted = claims.Any(claim =>
            claim.Type == Permissions.ClaimType &&
            claim.Value == permission);

        if (isGranted)
        {
            return Result.Success();
        }

        var identityResult = await _roleManager.AddClaimAsync(role, new Claim(Permissions.ClaimType, permission));

        return identityResult.ToApplicationResult();
    }

    public async Task<Result> RevokePermissionAsync(string roleName, string permission, CancellationToken cancellationToken = default)
    {
        var role = await _roleManager.FindByNameAsync(roleName);

        if (role == null)
        {
            return Result.Failure(RoleErrors.NotFound(roleName));
        }

        var claims = await _roleManager.GetClaimsAsync(role);

        var claim = claims.FirstOrDefault(claim =>
            claim.Type == Permissions.ClaimType &&
            claim.Value == permission);

        if (claim == null)
        {
            return Result.Success();
        }

        var identityResult = await _roleManager.RemoveClaimAsync(role, claim);

        return identityResult.ToApplicationResult();
    }
}
