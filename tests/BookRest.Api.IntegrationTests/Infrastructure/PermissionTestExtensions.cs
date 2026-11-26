using BookRest.Application.Common.Interfaces;
using BookRest.Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace BookRest.Api.IntegrationTests.Infrastructure;

internal static class PermissionTestExtensions
{
    /// <summary>
    /// Creates a user that belongs to a randomly generated role with given permissions.
    /// The user has no buisness roles.
    /// </summary>
    /// <param name="factory">The test web application factory.</param>
    /// <param name="permissions">An array of permission names.</param>
    /// <returns>The <see cref="string"/> email of created user.</returns>
    public static async Task<string> CreateUserWithPermissionsAsync(this BookRestWebApplicationFactory factory, params string[] permissions)
    {
        var guid = Guid.NewGuid();
        var roleName = $"role-{guid}";
        var userName = $"user-{guid}";
        var email = $"{guid}@gmail.com";
        var password = AuthenticationHttpClientExtensions.DefaultPassword;

        using var scope = factory.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var permissionService = scope.ServiceProvider.GetRequiredService<IPermissionService>();
        var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();

        var roleResult = await roleManager.CreateAsync(new ApplicationRole(roleName));

        if (!roleResult.Succeeded)
        {
            var errors = string.Join(
                separator: "| ",
                values: roleResult.Errors.Select(e => e.Description));

            throw new InvalidOperationException($"Failed to create the role: {errors}");
        }

        foreach (var permission in permissions)
        {
            var grantResult = await permissionService.GrantPermissionAsync(roleName, permission);

            if (grantResult.IsFailure)
            {
                var error = grantResult.Error?.Description;
                throw new InvalidOperationException($"Failed to grant permission '{permission}': {error}");
            }
        }

        var userResult = await identityService.CreateUserAsync(roleName, email, password);

        if (userResult.IsFailure)
        {
            var error = userResult.Error?.Description;
            throw new InvalidOperationException($"Failed to create the user: {error}");
        }

        var addToRoleResult = await identityService.AddToRoleAsync(userResult.Value, roleName);

        if (addToRoleResult.IsFailure)
        {
            var error = addToRoleResult.Error?.Description;
            throw new InvalidOperationException($"Failed to assign the role '{roleName}': {error}");
        }

        return email;
    }
}
