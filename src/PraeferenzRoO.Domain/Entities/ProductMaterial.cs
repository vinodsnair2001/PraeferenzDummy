using PraeferenzRoO.Domain.Common;

namespace PraeferenzRoO.Domain.Entities;

/// <summary>
/// Child entity that links a <see cref="FinishedProduct"/> to a <see cref="Material"/>.
/// Navigated exclusively through the <see cref="FinishedProduct"/> aggregate root —
/// no repository exists for this entity.
/// </summary>
public class ProductMaterial : AuditableEntity
{
    /// <summary>Gets or sets the tenant this record belongs to.</summary>
    public Guid TenantId { get; set; }

    /// <summary>Gets or sets the identifier of the owning finished product.</summary>
    public Guid FinishedProductId { get; set; }

    /// <summary>Gets or sets the identifier of the referenced material.</summary>
    public Guid MaterialId { get; set; }

    /// <summary>Gets or sets the quantity of the material used in the finished product.</summary>
    public decimal Quantity { get; set; }

    /// <summary>Gets or sets the total cost of this material line (Quantity × UnitCost).</summary>
    public decimal TotalCost { get; set; }
}
