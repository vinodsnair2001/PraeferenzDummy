using PraeferenzRoO.Domain.Common;
using PraeferenzRoO.Domain.Enums;

namespace PraeferenzRoO.Domain.Entities;

/// <summary>
/// Aggregate root representing a single preferential rule-of-origin definition.
/// Rules are stored in the database and loaded by the rule engine at evaluation time —
/// no business logic is hard-coded here.
/// </summary>
public class ProductRule : AggregateRoot
{
    /// <summary>Gets or sets the tenant this rule belongs to.</summary>
    public Guid TenantId { get; set; }

    /// <summary>Gets or sets the human-readable name of the rule.</summary>
    public string RuleName { get; set; } = string.Empty;

    /// <summary>Gets or sets the short code that uniquely identifies this rule within the system.</summary>
    public string RuleCode { get; set; } = string.Empty;

    /// <summary>Gets or sets whether this rule is mandatory, an alternative, or supplementary.</summary>
    public RuleCategory RuleCategory { get; set; }

    /// <summary>Gets or sets the type of origin criterion this rule implements.</summary>
    public RuleType RuleType { get; set; }

    /// <summary>Gets or sets the rule expression used by the engine evaluator, if applicable.</summary>
    public string? Expression { get; set; }

    /// <summary>Gets or sets the conditional expression that must be satisfied for this rule to apply.</summary>
    public string? Condition { get; set; }

    /// <summary>
    /// Gets or sets the JSON blob of rule-specific parameters.
    /// Schema is defined in the rule engine design (T09/T11).
    /// </summary>
    public string? ParametersJson { get; set; }

    /// <summary>Gets or sets the evaluation priority — lower values are evaluated first within the same execution order.</summary>
    public int Priority { get; set; }

    /// <summary>Gets or sets the position of this rule in the overall evaluation sequence.</summary>
    public int ExecutionOrder { get; set; }

    /// <summary>Gets or sets the date from which this rule is valid.</summary>
    public DateOnly EffectiveDate { get; set; }

    /// <summary>Gets or sets the date on which this rule expires, if applicable.</summary>
    public DateOnly? ExpiryDate { get; set; }

    /// <summary>Gets or sets the identifier of the trade agreement this rule belongs to.</summary>
    public Guid TradeAgreementId { get; set; }

    /// <summary>Gets or sets the identifier of a specific country this rule applies to, if scoped to one.</summary>
    public Guid? CountryId { get; set; }

    /// <summary>Gets or sets the identifier of the HS code chapter/heading this rule targets, if applicable.</summary>
    public Guid? HsCodeId { get; set; }

    /// <summary>Gets or sets a value indicating whether this rule is currently enabled for evaluation.</summary>
    public bool IsEnabled { get; set; }

    /// <summary>Gets or sets the version number of this rule definition, incremented on each update.</summary>
    public int Version { get; set; }
}
