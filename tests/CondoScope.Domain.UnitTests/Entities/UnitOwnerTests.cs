using CondoScope.Domain.Entities;

namespace CondoScope.Domain.UnitTests.Entities;

[TestClass]
public class UnitOwnerTests
{
    private static Unit CreateUnit(string unitNumber = "101")
    {
        var result = Unit.Create(unitNumber, "123 Main St", true, "creator");
        return result.Value;
    }

    private static Owner CreateOwner(string name = "John Doe")
    {
        var result = Owner.Create(name, null, null, "creator");
        return result.Value;
    }

    [TestMethod]
    public void AssignOwner_WithValidData_SetsAllProperties()
    {
        // Arrange
        var unit = CreateUnit();
        var owner = CreateOwner();
        var effectiveFrom = new DateOnly(2024, 1, 1);
        const string assignedBy = "tester";

        // Act
        unit.AssignOwner(owner, effectiveFrom, assignedBy);
        var unitOwner = unit.UnitOwners.Single();

        // Assert
        Assert.AreEqual(unit, unitOwner.Unit);
        Assert.AreEqual(owner, unitOwner.Owner);
        Assert.AreEqual(unit.Id, unitOwner.UnitId);
        Assert.AreEqual(owner.Id, unitOwner.OwnerId);
        Assert.AreEqual(effectiveFrom, unitOwner.EffectiveFrom);
        Assert.IsNull(unitOwner.EffectiveTo);
        Assert.AreEqual(assignedBy, unitOwner.CreatedBy);
        Assert.AreSame(unitOwner, owner.UnitOwner);
    }

    [TestMethod]
    public void AssignOwner_SecondOwner_AssignsEffectiveToOnPreviousOwner()
    {
        // Arrange
        var unit = CreateUnit();
        var owner1 = CreateOwner("Owner 1");
        var owner2 = CreateOwner("Owner 2");
        var effectiveFrom = new DateOnly(2024, 2, 1);
        var effectiveTo = new DateOnly(2024, 12, 31);

        unit.AssignOwner(owner1, effectiveFrom, "assigner1");

        // Act
        unit.AssignOwner(owner2, effectiveTo, "assigner2");
        var previousOwner = unit.UnitOwners.First(o => o.OwnerId == owner1.Id);

        // Assert
        Assert.AreEqual(effectiveTo, previousOwner.EffectiveTo);
    }

    [TestMethod]
    public void AssignOwner_GeneratesNonEmptyId()
    {
        // Arrange
        var unit = CreateUnit();
        var owner = CreateOwner();

        // Act
        unit.AssignOwner(owner, new DateOnly(2024, 3, 1), "creator");
        var unitOwner = unit.UnitOwners.Single();

        // Assert
        Assert.AreNotEqual(default, unitOwner.Id);
    }

    [TestMethod]
    public void AssignOwner_GeneratesUniqueIdsForDifferentInstances()
    {
        // Arrange
        var unit = CreateUnit();
        var owner1 = CreateOwner("Owner 1");
        var owner2 = CreateOwner("Owner 2");

        // Act
        unit.AssignOwner(owner1, new DateOnly(2024, 3, 1), "creator");
        unit.AssignOwner(owner2, new DateOnly(2024, 4, 1), "creator");

        var unitOwners = unit.UnitOwners.ToList();

        // Assert
        Assert.AreNotEqual(unitOwners[0].Id, unitOwners[1].Id);
    }

    [TestMethod]
    public void AssignOwner_SetsUnitIdFromUnit_NotIndependentValue()
    {
        // Arrange
        var unit1 = CreateUnit("101");
        var unit2 = CreateUnit("102");
        var owner = CreateOwner();

        // Act
        unit1.AssignOwner(owner, new DateOnly(2024, 4, 1), "creator");
        var unitOwner = unit1.UnitOwners.Single();

        // Assert
        Assert.AreEqual(unit1.Id, unitOwner.UnitId);
        Assert.AreNotEqual(unit2.Id, unitOwner.UnitId);
    }

    [TestMethod]
    public void AssignOwner_SetsOwnerIdFromOwner_NotIndependentValue()
    {
        // Arrange
        var unit = CreateUnit();
        var owner1 = CreateOwner("Owner 1");
        var owner2 = CreateOwner("Owner 2");

        // Act
        unit.AssignOwner(owner1, new DateOnly(2024, 4, 1), "creator");
        var unitOwner = unit.UnitOwners.Single();

        // Assert
        Assert.AreEqual(owner1.Id, unitOwner.OwnerId);
        Assert.AreNotEqual(owner2.Id, unitOwner.OwnerId);
    }
}
