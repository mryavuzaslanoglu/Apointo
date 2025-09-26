using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Apointo.Domain.Common;

public abstract class BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public Guid Id { get; protected set; } = Guid.NewGuid();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => new ReadOnlyCollection<IDomainEvent>(_domainEvents);

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}