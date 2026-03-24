using System.ComponentModel.DataAnnotations.Schema;

namespace Bookrest.Domain.Common;

public abstract class BaseEntity<T>
{
    // EF Core handles Id's by itself
    public T Id { get; set; } = default!;

    private List<BaseEvent> _domainEvents = new();

    [NotMapped] // don't map this field into migration
    public IReadOnlyCollection<BaseEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
