using PraeferenzRoO.Domain.Common;

namespace PraeferenzRoO.Domain.Entities;

/// <summary>
/// Represents a material or component used in the manufacture of a finished product.
/// Tracks originating status and unit cost for rules-of-origin value-added calculations.
/// </summary>
public class Material : AggregateRoot
{
    /// <summary>Gets or sets the tenant this material belongs to.</summary>
    public Guid TenantId { get; set; }

    /// <summary>Gets or sets the name of the material.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the HS tariff code associated with this material.</summary>
    public string HsCodeValue { get; set; } = string.Empty;

    /// <summary>Gets or sets the ISO 3166-1 alpha-2 code of the material's origin country.</summary>
    public string OriginCountryCode { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether this material qualifies as originating.</summary>
    public bool IsOriginating { get; set; }

    /// <summary>Gets or sets the unit cost of this material used in value-added calculations.</summary>
    public decimal UnitCost { get; set; }

    /// <summary>
    /// Gets or sets the ISO 4217 currency code (3 characters) for <see cref="UnitCost"/>.
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether this material record is active.</summary>
    public bool IsActive { get; set; }
}
