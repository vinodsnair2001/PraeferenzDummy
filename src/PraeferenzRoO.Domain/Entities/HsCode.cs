using PraeferenzRoO.Domain.Common;

namespace PraeferenzRoO.Domain.Entities;

/// <summary>
/// Represents an HS (Harmonized System) tariff code at any level of granularity.
/// Valid levels are 2, 4, 6, 8, and 10 digits.
/// </summary>
public class HsCode : AggregateRoot
{
    /// <summary>Gets or sets the tenant this HS code belongs to.</summary>
    public Guid TenantId { get; set; }

    /// <summary>Gets or sets the HS tariff code string (e.g., "8471", "847130").</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Gets or sets the human-readable description of this HS code.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the digit level of this code.
    /// Must be 2, 4, 6, 8, or 10 — validated via <see cref="IsValidLevel"/>.
    /// </summary>
    public int Level { get; set; }

    /// <summary>Gets or sets the parent code at the next higher level, if any.</summary>
    public string? ParentCode { get; set; }

    /// <summary>Gets or sets a value indicating whether this HS code entry is active.</summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Validates that <see cref="Level"/> is one of the permitted HS digit levels (2, 4, 6, 8, 10).
    /// </summary>
    /// <returns><c>true</c> if the level is valid; otherwise <c>false</c>.</returns>
    public bool IsValidLevel() => Level is 2 or 4 or 6 or 8 or 10;
}
