using BookRest.Application.Common.Interfaces;
using BookRest.Application.Common.Interfaces.Repositories;
using BookRest.Domain.Entities;
using BookRest.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BookRest.Infrastructure.Data.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly IApplicationDbContext _context;

    public CustomerRepository(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<Customer?> GetByApplicationUserIdAsync(Guid applicationUserId, CancellationToken cancellationToken = default)
    {
        return _context.Customers
            .FirstOrDefaultAsync(c => c.ApplicationUserId == applicationUserId, cancellationToken);
    }

    public Task<Customer?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return _context.Customers
            .FirstOrDefaultAsync(c => c.Email == email, cancellationToken);
    }

    public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        return _context.Customers
            .AnyAsync(c => c.Email == email, cancellationToken);
    }

    public void Add(Customer customer)
    {
        _context.Customers.Add(customer);
    }

    public void Update(Customer customer)
    {
        _context.Customers.Update(customer);
    }

    public void Remove(Customer customer)
    {
        _context.Customers.Remove(customer);
    }
}
