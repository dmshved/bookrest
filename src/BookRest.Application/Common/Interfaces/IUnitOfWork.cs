using BookRest.Domain.Shared;

namespace BookRest.Application.Common.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<Result> ExecuteInTransactionAsync(Func<Task<Result>> action, CancellationToken cancellationToken = default);
    Task<Result<T>> ExecuteInTransactionAsync<T>(Func<Task<Result<T>>> action, CancellationToken cancellationToken = default);
}
