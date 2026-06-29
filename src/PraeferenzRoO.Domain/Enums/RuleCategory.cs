namespace PraeferenzRoO.Domain.Enums;

/// <summary>
/// Indicates how a rule applies within a set of rules for a given HS code range.
/// </summary>
public enum RuleCategory
{
    Mandatory = 1,
    Alternative = 2,
    Supplementary = 3
}
