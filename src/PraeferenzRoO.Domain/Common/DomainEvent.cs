namespace PraeferenzRoO.Domain.Common;

/// <summary>
/// Base record for all domain events raised by aggregate roots.
/// Concrete domain event records inherit from this class and add event-specific data.
/// </summary>
public abstract record DomainEvent
{
    /// <summary>Gets the unique identifier of this event instance.</summary>
    public Guid EventId { get; } = Guid.NewGuid();

    /// <summary>Gets the UTC timestamp at which this event occurred.</summary>
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
