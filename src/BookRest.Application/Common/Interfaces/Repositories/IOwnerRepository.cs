using BookRest.Domain.Entities;
using BookRest.Domain.ValueObjects;

namespace BookRest.Application.Common.Interfaces.Repositories;

public interface IOwnerRepository
{
    Task<Owner?> GetByApplicationUserIdAsync(Guid applicationUserId, CancellationToken cancellationToken = default);
    Task<Owner?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

    void Add(Owner owner);
    void Update(Owner owner);
    void Remove(Owner owner);
}
