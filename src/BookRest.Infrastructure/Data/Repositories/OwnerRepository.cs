using BookRest.Application.Common.Interfaces;
using BookRest.Application.Common.Interfaces.Repositories;
using BookRest.Domain.Entities;
using BookRest.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BookRest.Infrastructure.Data.Repositories;

public class OwnerRepository : IOwnerRepository
{
    private readonly IApplicationDbContext _context;

    public OwnerRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Owner?> GetByApplicationUserIdAsync(Guid applicationUserId, CancellationToken cancellationToken = default)
    {
        return _context.Owners
            .FirstOrDefaultAsync(o => o.ApplicationUserId == applicationUserId, cancellationToken);
    }

    public Task<Owner?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return _context.Owners
            .FirstOrDefaultAsync(o => o.Email == email, cancellationToken);
    }

    public void Add(Owner owner)
    {
        _context.Owners.Add(owner);
    }

    public void Update(Owner owner)
    {
        _context.Owners.Update(owner);
    }

    public void Remove(Owner owner)
    {
        _context.Owners.Remove(owner);
    }
}
