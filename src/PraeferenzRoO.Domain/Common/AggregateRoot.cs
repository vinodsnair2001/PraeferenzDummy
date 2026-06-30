namespace PraeferenzRoO.Domain.Common;

/// <summary>
/// Base class for all aggregate roots in the domain.
/// Aggregate roots are the only entities that repositories operate on directly.
/// Provides infrastructure for collecting and clearing domain events.
/// </summary>
public abstract class AggregateRoot : AuditableEntity
{
    private readonly List<DomainEvent> _domainEvents = new();

    /// <summary>Gets the domain events raised by this aggregate since the last dispatch or clear.</summary>
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Registers a domain event to be dispatched after the unit of work is committed.
    /// </summary>
    /// <param name="domainEvent">The domain event to register.</param>
    protected void AddDomainEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>
    /// Clears all registered domain events. Called by the infrastructure layer after events have been dispatched.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
