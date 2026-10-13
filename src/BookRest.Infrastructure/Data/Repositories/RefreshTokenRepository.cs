using BookRest.Application.Common.Interfaces;
using BookRest.Application.Common.Interfaces.Repositories;
using BookRest.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookRest.Infrastructure.Data.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IApplicationDbContext _context;
    private readonly IClock _clock;

    public RefreshTokenRepository(
        IApplicationDbContext context,
        IClock clock)
    {
        _context = context;
        _clock = clock;
    }

    public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
    }

    public async Task RevokeAllActiveTokensAsync(Guid applicationUserId, string? reason = null, CancellationToken cancellationToken = default)
    {
        var now = _clock.GetCurrentInstant();

        var activeTokens = await _context.RefreshTokens
            .Where(t => t.ApplicationUserId == applicationUserId &&
                        t.IsActive(now))
            .ToListAsync(cancellationToken);

        foreach (var activeToken in activeTokens)
        {
            activeToken.Revoke(
                revokedAt: now,
                reason: reason);
        }
    }

    public void Add(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Add(refreshToken);
    }

    public void Update(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
    }

    public void Remove(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Remove(refreshToken);
    }
}
