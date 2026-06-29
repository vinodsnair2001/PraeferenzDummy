using PraeferenzRoO.Domain.Common;
using PraeferenzRoO.Domain.Entities;
using PraeferenzRoO.Domain.Enums;

namespace PraeferenzRoO.Domain.Tests;

/// <summary>
/// Unit tests for domain entities and aggregate root behaviour.
/// Covers: HsCode.IsValidLevel, AggregateRoot domain event infrastructure,
/// FinishedProduct.AddMaterial, and OriginCalculation.AddDetail.
/// </summary>
public class DomainEntityTests
{
    // ──────────────────────────────────────────────────────────────────────────
    // HsCode.IsValidLevel — valid values
    // ──────────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(6)]
    [InlineData(8)]
    [InlineData(10)]
    public void HsCode_IsValidLevel_Returns_True_For_Valid_Values(int level)
    {
        // Arrange
        var hsCode = new HsCode { Level = level };

        // Act
        var result = hsCode.IsValidLevel();

        // Assert
        Assert.True(result);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // HsCode.IsValidLevel — invalid values
    // ──────────────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(7)]
    [InlineData(11)]
    public void HsCode_IsValidLevel_Returns_False_For_Invalid_Values(int level)
    {
        // Arrange
        var hsCode = new HsCode { Level = level };

        // Act
        var result = hsCode.IsValidLevel();

        // Assert
        Assert.False(result);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // AggregateRoot — domain events accumulate and clear
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void AggregateRoot_DomainEvents_Accumulate_And_Clear()
    {
        // Arrange — use a concrete aggregate root to exercise the base infrastructure
        var product = new FinishedProduct();
        var testEvent1 = new TestDomainEvent();
        var testEvent2 = new TestDomainEvent();

        // Act — add events via the protected helper exposed through a test subclass
        var exposer = new AggregateRootEventExposer();
        exposer.RaiseEvent(testEvent1);
        exposer.RaiseEvent(testEvent2);

        // Assert — events accumulate
        Assert.Equal(2, exposer.DomainEvents.Count);

        // Act — clear
        exposer.ClearDomainEvents();

        // Assert — collection is empty after clear
        Assert.Empty(exposer.DomainEvents);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // FinishedProduct.AddMaterial
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void FinishedProduct_AddMaterial_Adds_To_Collection()
    {
        // Arrange
        var product = new FinishedProduct();
        var material = new ProductMaterial
        {
            FinishedProductId = product.Id,
            MaterialId = Guid.NewGuid(),
            Quantity = 3m,
            TotalCost = 150m
        };

        // Act
        product.AddMaterial(material);

        // Assert
        Assert.Single(product.Materials);
        Assert.Contains(material, product.Materials);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // OriginCalculation.AddDetail
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void OriginCalculation_AddDetail_Adds_To_Collection()
    {
        // Arrange
        var calculation = new OriginCalculation();
        var detail = new OriginCalculationDetail
        {
            OriginCalculationId = calculation.Id,
            RuleName = "Tariff Shift Rule",
            RuleType = RuleType.TariffShift,
            Passed = true,
            ExecutionOrder = 1
        };

        // Act
        calculation.AddDetail(detail);

        // Assert
        Assert.Single(calculation.Details);
        Assert.Contains(detail, calculation.Details);
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// Test helpers — private to the test assembly
// ──────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Concrete aggregate root used to exercise the domain event infrastructure
/// defined in <see cref="AggregateRoot"/> without coupling tests to a specific entity.
/// </summary>
file sealed class AggregateRootEventExposer : AggregateRoot
{
    public Guid TenantId { get; set; }

    /// <summary>Calls the protected <c>AddDomainEvent</c> method for test purposes.</summary>
    public void RaiseEvent(DomainEvent domainEvent) => AddDomainEvent(domainEvent);
}

/// <summary>Minimal concrete domain event used only in tests.</summary>
file sealed record TestDomainEvent : DomainEvent;
