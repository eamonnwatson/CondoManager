using CondoScope.Domain.Entities;
using CondoScope.Domain.Enums;

namespace CondoScope.Domain.UnitTests.Entities;

[TestClass]
public class FeeChargeTests
{
    [TestMethod]
    public void Create_WithValidParametersAndUnit_ReturnsSuccessResultWithExpectedValues()
    {
        // Arrange
        var unitResult = Unit.Create("101", "Main St", true, "creator");
        var unit = unitResult.Value;
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
        Assert.AreEqual(unit, feeCharge.Unit);
        Assert.AreEqual(unit.Id, feeCharge.UnitId);
        Assert.AreEqual(createdBy, feeCharge.CreatedBy);
        Assert.AreNotEqual(default(Ulid), feeCharge.Id);
    }

    [TestMethod]
    public void Create_WithNullUnit_ReturnsSuccessResultWithNullUnitAndNullUnitId()
    {
        // Arrange
        const decimal amount = 200m;
        var dueDate = new DateOnly(2024, 3, 1);
        const string description = "Common area fee";
        const ChargeCategory category = ChargeCategory.OtherFee;
        const ChargeScope scope = ChargeScope.AllUnits;
        const string createdBy = "system";

        // Act
        var result = FeeCharge.Create(amount, dueDate, description, category, scope, null, createdBy);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        var feeCharge = result.Value;
        Assert.IsNull(feeCharge.Unit);
        Assert.IsNull(feeCharge.UnitId);
        Assert.AreEqual(amount, feeCharge.Amount);
        Assert.AreEqual(description, feeCharge.Description);
        Assert.AreEqual(createdBy, feeCharge.CreatedBy);
    }

    [TestMethod]
    public void Create_SetsCreatedAtCloseToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var result = FeeCharge.Create(10m, DateOnly.FromDateTime(DateTime.UtcNow), "desc",
            ChargeCategory.CondoFee, ChargeScope.SpecificUnit, null, "user");

        var after = DateTime.UtcNow;

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.Value.CreatedAtUtc >= before && result.Value.CreatedAtUtc <= after);
    }

    [TestMethod]
    public void Create_WithZeroAmount_ReturnsSuccessResult()
    {
        // Arrange & Act
        var result = FeeCharge.Create(0m, new DateOnly(2024, 5, 1), "zero fee",
            ChargeCategory.CondoFee, ChargeScope.SpecificUnit, null, "user");

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0m, result.Value.Amount);
    }

    [TestMethod]
    public void Create_WithNegativeAmount_ReturnsSuccessResultWithNegativeAmount()
    {
        // Arrange & Act
        var result = FeeCharge.Create(-50m, new DateOnly(2024, 5, 1), "negative fee",
            ChargeCategory.CondoFee, ChargeScope.SpecificUnit, null, "user");

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(-50m, result.Value.Amount);
    }
}
