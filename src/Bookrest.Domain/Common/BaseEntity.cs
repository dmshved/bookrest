using System.ComponentModel.DataAnnotations.Schema;

namespace BookRest.Domain.Common;

public abstract class BaseEntity
{
    // EF Core uses conventions to process the Id
    public Guid Id { get; set; } 

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