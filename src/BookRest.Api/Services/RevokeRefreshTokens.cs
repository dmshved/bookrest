using BookRest.Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BookRest.Api.Services;

// Inject IHttpContextAccessor to be able to validate who is the current user
internal sealed class RevokeRefreshTokens(AppDbContext context, IHttpContextAccessor httpContextAccessor)
{
    public async Task<bool> Handle(string userId)
    {
        if (userId != GetCurrentUserId())
        {
            throw new ApplicationException("Access denied");
        }

        await context.RefreshTokens
            .Where(r => r.UserId == userId)
            .ExecuteDeleteAsync();

        return true;
    }

    private string? GetCurrentUserId()
    {
        return Guid.TryParse(
            httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier),
            out Guid parsed)
            ? parsed.ToString() : null;
    }
}
