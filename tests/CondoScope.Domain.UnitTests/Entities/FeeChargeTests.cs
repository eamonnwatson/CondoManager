using CondoScope.Domain.Entities;
using CondoScope.Domain.Enums;

namespace CondoScope.Domain.UnitTests.Entities;

[TestClass]
public class FeeChargeTests
{
    private static Unit CreateUnit(string unitNumber)
    {
        var unitResult = Unit.Create(unitNumber, "Main St", true, "creator");
        return unitResult.Value;
    }

    [TestMethod]
    public void Create_WithSpecificUnit_AssignsSingleUnit()
    {
        // Arrange
        var unit = CreateUnit("101");
        const decimal amount = 150.50m;
        var dueDate = new DateOnly(2024, 1, 15);
        const string description = "Monthly fee";
        const ChargeCategory category = ChargeCategory.CondoFee;
        const ChargeScope scope = ChargeScope.SpecificUnit;
        const string createdBy = "admin";

        // Act
        var result = FeeCharge.Create(amount, dueDate, description, category, scope, unit, createdBy);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        var feeCharge = result.Value;
        Assert.AreEqual(amount, feeCharge.Amount);
        Assert.AreEqual(dueDate, feeCharge.DueDate);
        Assert.AreEqual(description, feeCharge.Description);
        Assert.AreEqual(category, feeCharge.Category);
        Assert.AreEqual(scope, feeCharge.Scope);
        Assert.HasCount(1, feeCharge.Units);
        Assert.AreEqual(unit.Id, feeCharge.Units.Single().Id);
        Assert.AreEqual(createdBy, feeCharge.CreatedBy);
        Assert.AreNotEqual(default(Ulid), feeCharge.Id);
    }

    [TestMethod]
    public void Create_WithAllUnits_AssignsEveryProvidedUnit()
    {
        // Arrange
        var unit1 = CreateUnit("101");
        var unit2 = CreateUnit("102");

        // Act
        var result = FeeCharge.Create(200m, new DateOnly(2024, 3, 1), "Common area fee", ChargeCategory.OtherFee,
            ChargeScope.AllUnits, null, "system", [unit1, unit2]);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.HasCount(2, result.Value.Units);
        CollectionAssert.AreEquivalent(new[] { unit1.Id, unit2.Id }, result.Value.Units.Select(u => u.Id).ToArray());
    }

    [TestMethod]
    public void Create_WithAllUnitsWithoutUnits_ReturnsFailure()
    {
        // Act
        var result = FeeCharge.Create(200m, new DateOnly(2024, 3, 1), "Common area fee", ChargeCategory.OtherFee,
            ChargeScope.AllUnits, null, "system");

        // Assert
        Assert.IsTrue(result.IsFailed);
    }

    [TestMethod]
    public void Create_WithSpecificUnitAndNullUnit_ReturnsFailure()
    {
        // Act
        var result = FeeCharge.Create(10m, DateOnly.FromDateTime(DateTime.UtcNow), "desc",
            ChargeCategory.CondoFee, ChargeScope.SpecificUnit, null, "user");

        // Assert
        Assert.IsTrue(result.IsFailed);
    }

    [TestMethod]
    public void Create_WithAllUnitsAndSingleUnitParameter_ReturnsSuccessAndAssignsUnit()
    {
        // Arrange
        var unit = CreateUnit("101");

        // Act
        var result = FeeCharge.Create(10m, DateOnly.FromDateTime(DateTime.UtcNow), "desc",
            ChargeCategory.CondoFee, ChargeScope.AllUnits, unit, "user", [unit]);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.HasCount(1, result.Value.Units);
        Assert.AreEqual(unit.Id, result.Value.Units.Single().Id);
    }

    [TestMethod]
    public void Create_SetsCreatedAtCloseToUtcNow()
    {
        // Arrange
        var unit = CreateUnit("101");
        var before = DateTime.UtcNow;

        // Act
        var result = FeeCharge.Create(10m, DateOnly.FromDateTime(DateTime.UtcNow), "desc",
            ChargeCategory.CondoFee, ChargeScope.SpecificUnit, unit, "user");

        var after = DateTime.UtcNow;

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.Value.CreatedAtUtc >= before && result.Value.CreatedAtUtc <= after);
    }

    [TestMethod]
    public void Create_WithZeroAmount_ReturnsSuccessResult()
    {
        // Arrange
        var unit = CreateUnit("101");

        // Act
        var result = FeeCharge.Create(0m, new DateOnly(2024, 5, 1), "zero fee",
            ChargeCategory.CondoFee, ChargeScope.SpecificUnit, unit, "user");

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0m, result.Value.Amount);
    }

    [TestMethod]
    public void Create_WithNegativeAmount_ReturnsSuccessResultWithNegativeAmount()
    {
        // Arrange
        var unit = CreateUnit("101");

        // Act
        var result = FeeCharge.Create(-50m, new DateOnly(2024, 5, 1), "negative fee",
            ChargeCategory.CondoFee, ChargeScope.SpecificUnit, unit, "user");

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(-50m, result.Value.Amount);
    }
}
