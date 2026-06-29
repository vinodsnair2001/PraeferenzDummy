using PraeferenzRoO.Domain.Common;

namespace PraeferenzRoO.Domain.Entities;

/// <summary>
/// Represents a preferential trade agreement (e.g., EU-Korea FTA) under which
/// rules of origin are evaluated.
/// </summary>
public class TradeAgreement : AggregateRoot
{
    /// <summary>Gets or sets the tenant this trade agreement belongs to.</summary>
    public Guid TenantId { get; set; }

    /// <summary>Gets or sets the full name of the trade agreement.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the short code used to identify the agreement (e.g., "EU-KOR").</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional human-readable description of the agreement.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the date on which the agreement enters into force.</summary>
    public DateOnly EffectiveDate { get; set; }

    /// <summary>Gets or sets the date on which the agreement expires, if applicable.</summary>
    public DateOnly? ExpiryDate { get; set; }

    /// <summary>Gets or sets a value indicating whether this trade agreement is currently active.</summary>
    public bool IsActive { get; set; }
}
