using PraeferenzRoO.Domain.Common;

namespace PraeferenzRoO.Domain.Entities;

/// <summary>
/// Represents a country or territory in the rules-of-origin context.
/// Used to identify the origin of materials and the partner country of a trade agreement.
/// </summary>
public class Country : AggregateRoot
{
    /// <summary>Gets or sets the tenant this country record belongs to.</summary>
    public Guid TenantId { get; set; }

    /// <summary>Gets or sets the full country name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the ISO 3166-1 alpha-2 country code (2 characters).</summary>
    public string IsoCode2 { get; set; } = string.Empty;

    /// <summary>Gets or sets the ISO 3166-1 alpha-3 country code (3 characters).</summary>
    public string IsoCode3 { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether this country is an EU member state.</summary>
    public bool IsEuMember { get; set; }

    /// <summary>Gets or sets a value indicating whether this country record is active.</summary>
    public bool IsActive { get; set; }
}
