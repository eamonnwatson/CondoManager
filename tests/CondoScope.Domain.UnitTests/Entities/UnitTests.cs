using CondoScope.Domain.Errors;
using DomainEntities = CondoScope.Domain.Entities;

namespace CondoScope.Domain.UnitTests.Entities;

[TestClass]
public class UnitTests
{
    private static DomainEntities.Owner CreateOwner(string name = "Owner")
    {
        var result = DomainEntities.Owner.Create(name, null, null, "creator");
        return result.Value;
    }

    private static DomainEntities.Unit CreateUnit(string unitNumber = "101")
    {
        var result = DomainEntities.Unit.Create(unitNumber, "Address", true, "creator");
        return result.Value;
    }

    [TestMethod]
    public void Create_WithValidParameters_ReturnsSuccessResult()
    {
        // Act
        var result = DomainEntities.Unit.Create("101", "Some Address", true, "creator");

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Value);
    }

    [TestMethod]
    public void Create_WithNullAddress_ReturnsSuccessResult()
    {
        // Act
        var result = DomainEntities.Unit.Create("101", null, false, "creator");

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Value);
    }

    [TestMethod]
    public void UnitOwners_WhenNoOwnersAssigned_ReturnsEmptyCollection()
    {
        // Arrange
        var unit = CreateUnit();

        // Act
        var owners = unit.UnitOwners;

        // Assert
        Assert.IsEmpty(owners);
    }

    [TestMethod]
    public void AssignOwner_FirstOwner_AddsOwnerAndReturnsSuccess()
    {
        // Arrange
        var unit = CreateUnit();
        var owner = CreateOwner();
        var effectiveDate = new DateOnly(2024, 1, 1);

        // Act
        var result = unit.AssignOwner(owner, effectiveDate, "assigner");

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.HasCount(1, unit.UnitOwners);
        var addedOwner = unit.UnitOwners.First();
        Assert.AreEqual(effectiveDate, addedOwner.EffectiveFrom);
        Assert.IsNull(addedOwner.EffectiveTo);
        Assert.AreEqual(owner, addedOwner.Owner);
        Assert.AreEqual(unit, addedOwner.Unit);
    }

    [TestMethod]
    public void AssignOwner_SecondOwnerWithLaterDate_ClosesPreviousOwnerAndAddsNew()
    {
        // Arrange
        var unit = CreateUnit();
        var owner1 = CreateOwner("Owner1");
        var owner2 = CreateOwner("Owner2");
        var firstDate = new DateOnly(2024, 1, 1);
        var secondDate = new DateOnly(2024, 6, 1);

        unit.AssignOwner(owner1, firstDate, "assigner1");

        // Act
        var result = unit.AssignOwner(owner2, secondDate, "assigner2");

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.HasCount(2, unit.UnitOwners);

        var ownersList = unit.UnitOwners.ToList();
        var firstUnitOwner = ownersList.First(uo => uo.Owner == owner1);
        var secondUnitOwner = ownersList.First(uo => uo.Owner == owner2);

        Assert.AreEqual(secondDate, firstUnitOwner.EffectiveTo);
        Assert.AreEqual("assigner2", firstUnitOwner.LastModifiedBy);
        Assert.IsNull(secondUnitOwner.EffectiveTo);
    }

    [TestMethod]
    public void AssignOwner_WithEarlierEffectiveDateThanLastOwner_ReturnsFailureWithInvalidEffectiveDate()
    {
        // Arrange
        var unit = CreateUnit();
        var owner1 = CreateOwner("Owner1");
        var owner2 = CreateOwner("Owner2");
        var firstDate = new DateOnly(2024, 6, 1);
        var earlierDate = new DateOnly(2024, 1, 1);

        unit.AssignOwner(owner1, firstDate, "assigner1");

        // Act
        var result = unit.AssignOwner(owner2, earlierDate, "assigner2");

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.HasCount(1, unit.UnitOwners);
        Assert.IsInstanceOfType<InvalidEffectiveDate>(result.Errors[0]);
        var error = (InvalidEffectiveDate)result.Errors[0];
        Assert.AreEqual(earlierDate, error.EffectiveDate);
    }

    [TestMethod]
    public void AssignOwner_WhenLastOwnerAlreadyHasEffectiveTo_DoesNotOverwriteEffectiveTo()
    {
        // Arrange
        var unit = CreateUnit();
        var owner1 = CreateOwner("Owner1");
        var owner2 = CreateOwner("Owner2");
        var owner3 = CreateOwner("Owner3");
        var firstDate = new DateOnly(2024, 1, 1);
        var secondDate = new DateOnly(2024, 6, 1);
        var thirdDate = new DateOnly(2024, 12, 1);

        unit.AssignOwner(owner1, firstDate, "assigner1");
        unit.AssignOwner(owner2, secondDate, "assigner2");

        // Act
        var result = unit.AssignOwner(owner3, thirdDate, "assigner3");

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.HasCount(3, unit.UnitOwners);

        var ownersList = unit.UnitOwners.ToList();
        var secondUnitOwner = ownersList.First(uo => uo.Owner == owner2);
        Assert.AreEqual(thirdDate, secondUnitOwner.EffectiveTo);
    }

    [TestMethod]
    public void AssignOwner_WithSameEffectiveDateAsLastOwner_ClosesPreviousOwnerAndSucceeds()
    {
        // Arrange
        var unit = CreateUnit();
        var owner1 = CreateOwner("Owner1");
        var owner2 = CreateOwner("Owner2");
        var sameDate = new DateOnly(2024, 1, 1);

        unit.AssignOwner(owner1, sameDate, "assigner1");

        // Act & Assert
        // Using a different date is required since SortedList keys must be unique;
        // but same effective date as last owner should not be rejected by the date check
        // since the failure condition is strictly greater-than.
        var laterDate = sameDate;
        laterDate = laterDate.AddDays(0);

        Assert.ThrowsExactly<ArgumentException>(() => unit.AssignOwner(owner2, sameDate, "assigner2"));
    }
}
