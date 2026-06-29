namespace PraeferenzRoO.Domain.Common;

/// <summary>
/// Root base class for all domain entities.
/// Provides the primary key identity for every entity in the system.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>Gets the unique identifier for this entity.</summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();
}
