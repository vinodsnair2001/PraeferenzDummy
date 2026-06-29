using PraeferenzRoO.Domain.Common;
using PraeferenzRoO.Domain.Enums;

namespace PraeferenzRoO.Domain.Entities;

/// <summary>
/// Child entity that records the evaluation outcome for a single rule within an origin calculation.
/// Navigated exclusively through the <see cref="OriginCalculation"/> aggregate root —
/// no repository exists for this entity.
/// </summary>
public class OriginCalculationDetail : AuditableEntity
{
    /// <summary>Gets or sets the tenant this detail record belongs to.</summary>
    public Guid TenantId { get; set; }

    /// <summary>Gets or sets the identifier of the owning origin calculation.</summary>
    public Guid OriginCalculationId { get; set; }

    /// <summary>Gets or sets the name of the rule that was evaluated.</summary>
    public string RuleName { get; set; } = string.Empty;

    /// <summary>Gets or sets the type of rule that was evaluated.</summary>
    public RuleType RuleType { get; set; }

    /// <summary>Gets or sets a value indicating whether this rule evaluation passed.</summary>
    public bool Passed { get; set; }

    /// <summary>Gets or sets an optional human-readable message explaining the evaluation result.</summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the JSON blob containing the evidence data that supported this evaluation.
    /// Schema is defined in the rule engine design (T11).
    /// </summary>
    public string? EvidenceJson { get; set; }

    /// <summary>Gets or sets the position of this rule in the evaluation sequence.</summary>
    public int ExecutionOrder { get; set; }
}
