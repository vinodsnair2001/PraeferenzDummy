namespace PraeferenzRoO.Domain.Common;

/// <summary>
/// Extends <see cref="BaseEntity"/> with a full audit trail.
/// Every persisted entity must inherit from this class to satisfy the mandatory audit requirement.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    /// <summary>Gets or sets the username or system identity that created this record.</summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>Gets or sets the username or system identity that last updated this record.</summary>
    public string? UpdatedBy { get; set; }

    /// <summary>Gets or sets the username or system identity that soft-deleted this record.</summary>
    public string? DeletedBy { get; set; }

    /// <summary>Gets or sets the UTC date and time at which this record was created.</summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>Gets or sets the UTC date and time at which this record was last modified.</summary>
    public DateTime? ModifiedDate { get; set; }

    /// <summary>Gets or sets the UTC date and time at which this record was soft-deleted.</summary>
    public DateTime? DeletedDate { get; set; }

    /// <summary>Gets or sets a value indicating whether this record has been soft-deleted.</summary>
    public bool IsDeleted { get; set; }

    /// <summary>Gets or sets the IP address of the client that performed the last write.</summary>
    public string? IPAddress { get; set; }

    /// <summary>Gets or sets the machine name that performed the last write.</summary>
    public string? Machine { get; set; }
}
