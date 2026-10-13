using BookRest.Application.Common.Interfaces;
using BookRest.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BookRest.Infrastructure.Data.Interceptors;

public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IClock _clock;

    public AuditableEntityInterceptor(ICurrentUserService currentUserService, IClock clock)
    {
        _currentUserService = currentUserService;
        _clock = clock;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public void UpdateEntities(DbContext? context)
    {
        if (context == null)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries<BaseAuditableEntity>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified || entry.HasChangedOwnedEntities())
            {
                var now = _clock.GetCurrentInstant();

                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedBy = _currentUserService.ApplicationUserId;
                    entry.Entity.CreatedAt = now;
                }

                entry.Entity.ModifiedBy = _currentUserService.ApplicationUserId;
                entry.Entity.ModifiedAt = now;
            }
        }
    }
}

// More about accessing Tracked Entities: https://learn.microsoft.com/en-us/ef/core/change-tracking/entity-entries
public static class Extensions
{
    public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
        entry.References.Any(r =>
            r.TargetEntry != null &&
            r.TargetEntry.Metadata.IsOwned() &&
            (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
}
