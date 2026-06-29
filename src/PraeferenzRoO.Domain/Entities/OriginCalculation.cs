using PraeferenzRoO.Domain.Common;
using PraeferenzRoO.Domain.Enums;

namespace PraeferenzRoO.Domain.Entities;

/// <summary>
/// Aggregate root representing a single origin determination run for a finished product
/// under a specific trade agreement and destination country.
/// Manages its evaluation detail collection via encapsulated domain methods.
/// </summary>
public class OriginCalculation : AggregateRoot
{
    private readonly List<OriginCalculationDetail> _details = new();

    /// <summary>Gets or sets the tenant this calculation belongs to.</summary>
    public Guid TenantId { get; set; }

    /// <summary>Gets or sets the identifier of the finished product being assessed.</summary>
    public Guid FinishedProductId { get; set; }

    /// <summary>Gets or sets the identifier of the trade agreement under which origin is assessed.</summary>
    public Guid TradeAgreementId { get; set; }

    /// <summary>Gets or sets the identifier of the destination or partner country.</summary>
    public Guid CountryId { get; set; }

    /// <summary>Gets or sets the current lifecycle status of this calculation.</summary>
    public CalculationStatus Status { get; set; }

    /// <summary>Gets or sets whether the product is determined to be originating. Null until a decision is reached.</summary>
    public bool? IsOriginating { get; set; }

    /// <summary>Gets or sets the human-readable summary of the origin determination decision.</summary>
    public string? DecisionSummary { get; set; }

    /// <summary>
    /// Gets or sets the JSON representation of the full decision tree produced by the rule engine.
    /// Schema is defined in the rule engine design (T11).
    /// </summary>
    public string? DecisionTreeJson { get; set; }

    /// <summary>Gets or sets the UTC date/time at which the calculation completed.</summary>
    public DateTime? CalculatedAt { get; set; }

    /// <summary>Gets the read-only view of rule evaluation details for this calculation.</summary>
    public IReadOnlyCollection<OriginCalculationDetail> Details => _details.AsReadOnly();

    /// <summary>
    /// Adds a rule evaluation detail record to this calculation.
    /// </summary>
    /// <param name="detail">The detail to add.</param>
    public void AddDetail(OriginCalculationDetail detail) => _details.Add(detail);
}
