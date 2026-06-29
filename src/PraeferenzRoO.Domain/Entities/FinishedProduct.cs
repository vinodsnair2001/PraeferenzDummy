using PraeferenzRoO.Domain.Common;

namespace PraeferenzRoO.Domain.Entities;

/// <summary>
/// Aggregate root representing the manufactured product whose preferential origin is being assessed.
/// Manages its bill-of-materials collection via encapsulated domain methods.
/// </summary>
public class FinishedProduct : AggregateRoot
{
    private readonly List<ProductMaterial> _materials = new();

    /// <summary>Gets or sets the tenant this product belongs to.</summary>
    public Guid TenantId { get; set; }

    /// <summary>Gets or sets the name of the finished product.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the HS tariff code of the finished product.</summary>
    public string HsCodeValue { get; set; } = string.Empty;

    /// <summary>Gets or sets the ex-works price of the finished product used in value-added calculations.</summary>
    public decimal ExWorkPrice { get; set; }

    /// <summary>
    /// Gets or sets the ISO 4217 currency code (3 characters) for <see cref="ExWorkPrice"/>.
    /// </summary>
    public string Currency { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether this finished product record is active.</summary>
    public bool IsActive { get; set; }

    /// <summary>Gets the read-only view of materials in this product's bill of materials.</summary>
    public IReadOnlyCollection<ProductMaterial> Materials => _materials.AsReadOnly();

    /// <summary>
    /// Adds a material to this product's bill of materials.
    /// </summary>
    /// <param name="material">The product-material association to add.</param>
    public void AddMaterial(ProductMaterial material) => _materials.Add(material);
}
