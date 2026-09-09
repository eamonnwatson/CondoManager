using CondoScope.Domain.Entities;

namespace CondoScope.Domain.UnitTests.Entities;

[TestClass]
public class UnitOwnerTests
{
    private static Unit CreateUnit()
    {
        var result = Unit.Create("101", "123 Main St", true, "creator");
        return result.Value;
    }

    private static Owner CreateOwner()
    {
        var result = Owner.Create("John Doe", null, null, "creator");
        return result.Value;
    }

    [TestMethod]
    public void Constructor_WithValidData_SetsAllProperties()
    {
        // Arrange
        var unit = CreateUnit();
        var owner = CreateOwner();
        var effectiveFrom = new DateOnly(2024, 1, 1);
        var effectiveTo = new DateOnly(2024, 12, 31);
        var createdAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        const string createdBy = "tester";

        // Act
        var unitOwner = new UnitOwner(unit, owner, effectiveFrom, effectiveTo, createdAt, createdBy);

        // Assert
        Assert.AreEqual(unit, unitOwner.Unit);
        Assert.AreEqual(owner, unitOwner.Owner);
        Assert.AreEqual(unit.Id, unitOwner.UnitId);
        Assert.AreEqual(owner.Id, unitOwner.OwnerId);
        Assert.AreEqual(effectiveFrom, unitOwner.EffectiveFrom);
        Assert.AreEqual(effectiveTo, unitOwner.EffectiveTo);
        Assert.AreEqual(createdAt, unitOwner.CreatedAtUtc);
        Assert.AreEqual(createdBy, unitOwner.CreatedBy);
    }

    [TestMethod]
    public void Constructor_WithNullEffectiveTo_SetsEffectiveToNull()
    {
        // Arrange
        var unit = CreateUnit();
        var owner = CreateOwner();
        var effectiveFrom = new DateOnly(2024, 2, 1);

        // Act
        var unitOwner = new UnitOwner(unit, owner, effectiveFrom, null, DateTime.UtcNow, "creator");

        // Assert
        Assert.IsNull(unitOwner.EffectiveTo);
    }

    [TestMethod]
    public void Constructor_GeneratesNonEmptyId()
    {
        // Arrange
        var unit = CreateUnit();
        var owner = CreateOwner();

        // Act
        var unitOwner = new UnitOwner(unit, owner, new DateOnly(2024, 3, 1), null, DateTime.UtcNow, "creator");

        // Assert
        Assert.AreNotEqual(default, unitOwner.Id);
    }

    [TestMethod]
    public void Constructor_GeneratesUniqueIdsForDifferentInstances()
    {
        // Arrange
        var unit = CreateUnit();
        var owner = CreateOwner();

        // Act
        var unitOwner1 = new UnitOwner(unit, owner, new DateOnly(2024, 3, 1), null, DateTime.UtcNow, "creator");
        var unitOwner2 = new UnitOwner(unit, owner, new DateOnly(2024, 3, 1), null, DateTime.UtcNow, "creator");

        // Assert
        Assert.AreNotEqual(unitOwner1.Id, unitOwner2.Id);
    }

    [TestMethod]
    public void Constructor_SetsUnitIdFromUnit_NotIndependentValue()
    {
        // Arrange
        var unit1 = CreateUnit();
        var unit2 = CreateUnit();
        var owner = CreateOwner();

        // Act
        var unitOwner = new UnitOwner(unit1, owner, new DateOnly(2024, 4, 1), null, DateTime.UtcNow, "creator");

        // Assert
        Assert.AreEqual(unit1.Id, unitOwner.UnitId);
        Assert.AreNotEqual(unit2.Id, unitOwner.UnitId);
    }

    [TestMethod]
    public void Constructor_SetsOwnerIdFromOwner_NotIndependentValue()
    {
        // Arrange
        var unit = CreateUnit();
        var owner1 = CreateOwner();
        var owner2 = CreateOwner();

        // Act
        var unitOwner = new UnitOwner(unit, owner1, new DateOnly(2024, 4, 1), null, DateTime.UtcNow, "creator");

        // Assert
        Assert.AreEqual(owner1.Id, unitOwner.OwnerId);
        Assert.AreNotEqual(owner2.Id, unitOwner.OwnerId);
    }
}
