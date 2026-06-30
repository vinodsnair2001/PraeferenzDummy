namespace PraeferenzRoO.Domain.Enums;

/// <summary>
/// Tracks the lifecycle state of an <see cref="Entities.OriginCalculation"/>.
/// </summary>
public enum CalculationStatus
{
    Pending = 1,
    InProgress = 2,
    Originating = 3,
    NonOriginating = 4,
    Error = 5
}
