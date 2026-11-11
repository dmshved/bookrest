using BookRest.Application.Common.Interfaces;
using BookRest.Application.Common.Interfaces.Repositories;
using BookRest.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace BookRest.Api.IntegrationTests.Common;

internal static class BookRestWebApplicationFactoryExtensions
{
    public const string DefaultPassword = AuthenticationHttpClientExtensions.DefaultPassword;

    /// <summary>
    /// Creates the user with provided role.
    /// </summary>
    /// <param name="factory">The test web application factory.</param>
    /// <param name="roleName">The name of the role.</param>
    /// <param name="password">The password.</param>
    /// <returns>Email address of the created user.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the user cannot be created or assigned to a role.</exception>
    public static async Task<string> CreateUserWithRole(this BookRestWebApplicationFactory factory, string roleName, string password = DefaultPassword)
    {
        // Generate unique identifier and paste it into email and username
        var guid = Guid.NewGuid();
        var userName = $"user-{guid}";
        var email = $"{guid}@gmail.com";

        using var scope = factory.Services.CreateScope();
        var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();

        var userResult = await identityService.CreateUserAsync(userName, email, password);

        if (userResult.IsFailure)
        {
            throw new InvalidOperationException($"Failed to create the user: {userResult.Error?.Description}");
        }

        var roleResult = await identityService.AddToRoleAsync(userResult.Value, roleName);

        if (roleResult.IsFailure)
        {
            throw new InvalidOperationException($"Failed to assign the user to the role: {roleResult.Error?.Description}");
        }

        return email;
    }

    /// <summary>
    /// Returns refresh token stored in the database.
    /// </summary>
    /// <param name="factory">The test web application factory.</param>
    /// <param name="refreshTokenValue">The refresh token value.</param>
    /// <returns>A <see cref="RefreshToken"/> instance.</returns>
    public static async Task<RefreshToken?> GetStoredRefreshTokenAsync(this BookRestWebApplicationFactory factory, string refreshTokenValue)
    {
        using var scope = factory.Services.CreateScope();
        var refreshTokenRepository = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();

        return await refreshTokenRepository.GetByTokenAsync(refreshTokenValue);
    }
}
