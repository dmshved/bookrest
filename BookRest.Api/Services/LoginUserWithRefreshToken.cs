using BookRest.Api.Data;
using BookRest.Api.Infrastructure;
using BookRest.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookRest.Api.Services;

internal class LoginUserWithRefreshToken(AppDbContext context, JwtTokenProvider jwtTokenProvider, UserManager<AppUser> userManager)
{
    public sealed record Request(string RefreshToken);
    public sealed record Response(string AccessToken, string RefreshToken);

    public async Task<Response> Handle(Request request)
    {
        // Get the refresh token from the database
        RefreshToken? refreshToken = await context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == request.RefreshToken);

        // If token not found it is invalid
        if (refreshToken is null)
        {
            throw new ApplicationException("The refresh token is invalid");
        }

        // If the tokens expiration date is less than today's date token expired 
        if (refreshToken.ExpiresOnUtc < DateTime.UtcNow)
        {
            throw new ApplicationException("The refresh token has expired");
        }

        // Get all the tokens for the client and check if the token
        // from request is the latest one
        bool isLatestToken = await context.RefreshTokens
            .Where(r => r.UserId == refreshToken.UserId)
            .OrderByDescending(r => r.CreatedOnUtc)
            .Select(r => r.Token)
            .FirstOrDefaultAsync() == refreshToken.Token;

        // If token isn't the latest provide respective error message
        if (isLatestToken == false)
        {
            throw new ApplicationException("Access denied");
        }

        // Feth the roles from the user
        var roles = await userManager.GetRolesAsync(refreshToken.User);

        // Generate new access token
        string accessToken = jwtTokenProvider.GenerateAccessToken(refreshToken.User, roles);

        // Generate new refresh token
        refreshToken.Token = jwtTokenProvider.GenerateRefreshToken();
        // Update the expiration time
        //
        // Should I use some kind of options pattern to be
        // able to configure the expiration date for refreshToken?
        refreshToken.ExpiresOnUtc = DateTime.UtcNow.AddDays(7);

        await context.SaveChangesAsync();

        return new Response(accessToken, refreshToken.Token);
    }
}
