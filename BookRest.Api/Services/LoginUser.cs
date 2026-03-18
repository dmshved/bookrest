// !!! ATTENTION !!!
// Currently at the lines 33 loosing useful Identity exceptions
// Potential solution: custom Result<T> type
// example as follows
//public class Result<T>
//{
//    public bool Success { get; init; }
//    public T? Value { get; init; }
//    public List<string> Errors { get; init; } = [];
//}

using BookRest.Api.Data;
using BookRest.Api.Infrastructure;
using BookRest.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace BookRest.Api.Features;

internal sealed class LoginUser(AppDbContext context, UserManager<AppUser> userManager, JwtTokenProvider tokenProvider)
{
    public record class Request(string Email, string Password);
    public record class Response(string AccessToken, string RefreshToken);

    public async Task<Response> Handle(Request request)
    {
        // Fetch the user from the database
        var user = await userManager.FindByEmailAsync(request.Email);

        // Check if the user is null or password is not valid
        if (user is null ||
            await userManager.CheckPasswordAsync(user, request.Password) == false)
        {
            throw new ApplicationException("Error");
        }

        // Add roles as the claims into JWT
        var roles = await userManager.GetRolesAsync(user);

        // Generate the access token
        var accessToken = tokenProvider.GenerateAccessToken(user, roles);

        // Generate the refresh token
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = tokenProvider.GenerateRefreshToken(),
            ExpiresOnUtc = DateTime.UtcNow.AddDays(7),
            CreatedOnUtc = DateTime.UtcNow,
        };

        context.RefreshTokens.Add(refreshToken);

        await context.SaveChangesAsync();

        return new Response(accessToken, refreshToken.Token);
    }
}
