namespace PraeferenzRoO.Domain.Enums;

/// <summary>
/// Classifies the type of a preferential rules-of-origin rule.
/// Used by the rule engine to select the correct <c>IRule</c> evaluator.
/// </summary>
public enum RuleType
{
    TariffShift = 1,
    ValueAdded = 2,
    SpecificProcess = 3,
    WhollyObtained = 4,
    Cumulation = 5,
    Tolerance = 6,
    Combined = 7
}
