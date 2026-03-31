namespace BookRest.Application.Common.Interfaces;

interface IApplicationDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
