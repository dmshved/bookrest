using BookRest.Domain.Entities;

namespace BookRest.Application.Common.Interfaces;

interface IApplicationDbContext
{
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}