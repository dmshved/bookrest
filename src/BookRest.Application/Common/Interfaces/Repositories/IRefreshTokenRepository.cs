using BookRest.Domain.Entities;

namespace BookRest.Application.Common.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task RevokeAllActiveTokensAsync(Guid applicationUserId, string? reason = null, CancellationToken cancellationToken = default);

    void Add(RefreshToken refreshToken);
    void Update(RefreshToken refreshToken);
    void Remove(RefreshToken refreshToken);
}
