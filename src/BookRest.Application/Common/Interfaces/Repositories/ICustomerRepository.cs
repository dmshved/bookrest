using BookRest.Domain.Entities;
using BookRest.Domain.ValueObjects;

namespace BookRest.Application.Common.Interfaces.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByApplicationUserIdAsync(Guid applicationUserId, CancellationToken cancellationToken = default);
    Task<Customer?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

    void Add(Customer customer);
    void Update(Customer customer);
    void Remove(Customer customer);
}
