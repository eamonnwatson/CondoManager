using CondoScope.Domain.Entities;
using CondoScope.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CondoScope.Persistence.IntegrationTests;

[TestClass]
public class UnitPersistenceTests
{
    [TestMethod]
    public void Add_WhenOwnerAssignedToTwoDifferentUnits_ThrowsInvalidOperationException()
    {
        // Arrange
        var (context, connection) = SqliteTestDbContextFactory.Create();
        using (connection)
        using (context)
        {
            var unit1 = Unit.Create("101", "Address 1", true, "creator").Value;
            var unit2 = Unit.Create("102", "Address 2", true, "creator").Value;
            var owner = Owner.Create("Owner", null, null, "creator").Value;

            context.Units.AddRange(unit1, unit2);
            context.Owners.Add(owner);
            context.SaveChanges();

            var unitOwner1 = new UnitOwner(unit1, owner, new DateOnly(2024, 1, 1), null, DateTime.UtcNow, "creator");
            context.UnitOwners.Add(unitOwner1);
            context.SaveChanges();

            // Act & Assert
            var unitOwner2 = new UnitOwner(unit2, owner, new DateOnly(2024, 6, 1), null, DateTime.UtcNow, "creator");
            Assert.ThrowsExactly<InvalidOperationException>(() => context.UnitOwners.Add(unitOwner2));
        }
    }

    [TestMethod]
    public void SaveChanges_WhenSameUnitHasTwoUnitOwnersWithSameEffectiveFrom_ThrowsDbUpdateException()
    {
        // Arrange
        var (context, connection) = SqliteTestDbContextFactory.Create();
        using (connection)
        using (context)
        {
            var unit = Unit.Create("101", "Address 1", true, "creator").Value;
            var owner1 = Owner.Create("Owner1", null, null, "creator").Value;
            var owner2 = Owner.Create("Owner2", null, null, "creator").Value;

            context.Units.Add(unit);
            context.Owners.AddRange(owner1, owner2);
            context.SaveChanges();

            var effectiveDate = new DateOnly(2024, 1, 1);
            var unitOwner1 = new UnitOwner(unit, owner1, effectiveDate, null, DateTime.UtcNow, "creator");
            context.UnitOwners.Add(unitOwner1);
            context.SaveChanges();

            // Act
            var unitOwner2 = new UnitOwner(unit, owner2, effectiveDate, null, DateTime.UtcNow, "creator");
            context.UnitOwners.Add(unitOwner2);

            // Assert
            Assert.ThrowsExactly<DbUpdateException>(() => context.SaveChanges());
        }
    }

    [TestMethod]
    public void Include_WhenUnitHasPaymentsAndFeeCharges_LoadsNavigationsAfterReload()
    {
        // Arrange
        var (context, connection) = SqliteTestDbContextFactory.Create();
        using (connection)
        using (context)
        {
            var unit = Unit.Create("101", "Address 1", true, "creator").Value;
            context.Units.Add(unit);
            context.SaveChanges();

            var payment = Payment.Create(new DateOnly(2024, 1, 15), unit, 100m, PaymentMethod.Cash, "REF1", null, "creator").Value;
            var feeCharge = FeeCharge.Create(50m, new DateOnly(2024, 2, 1), "Monthly Fee", ChargeCategory.CondoFee, ChargeScope.SpecificUnit, unit, "creator").Value;

            context.Payments.Add(payment);
            context.FeeCharges.Add(feeCharge);
            context.SaveChanges();

            // Act
            var reloadedUnit = context.Units
                .Include(u => u.Payments)
                .Include(u => u.FeeCharges)
                .Single(u => u.Id == unit.Id);

            // Assert
            Assert.HasCount(1, reloadedUnit.Payments);
            Assert.HasCount(1, reloadedUnit.FeeCharges);
            Assert.AreEqual(payment.Id, reloadedUnit.Payments.First().Id);
            Assert.AreEqual(feeCharge.Id, reloadedUnit.FeeCharges.First().Id);
        }
    }

    [TestMethod]
    public void Include_WhenFeeChargeScopeIsAllUnits_LoadsFeeChargeForEveryUnit()
    {
        // Arrange
        var (context, connection) = SqliteTestDbContextFactory.Create();
        using (connection)
        using (context)
        {
            var unit1 = Unit.Create("101", "Address 1", true, "creator").Value;
            var unit2 = Unit.Create("102", "Address 2", true, "creator").Value;
            context.Units.AddRange(unit1, unit2);
            context.SaveChanges();

            var feeCharge = FeeCharge.Create(50m, new DateOnly(2024, 2, 1), "Monthly Fee", ChargeCategory.CondoFee,
                ChargeScope.AllUnits, null, "creator", [unit1, unit2]).Value;
            context.FeeCharges.Add(feeCharge);
            context.SaveChanges();

            // Act
            var reloadedUnits = context.Units
                .Include(u => u.FeeCharges)
                .OrderBy(u => u.UnitNumber)
                .ToList();

            // Assert
            Assert.HasCount(2, reloadedUnits);
            Assert.AreEqual(feeCharge.Id, reloadedUnits[0].FeeCharges.Single().Id);
            Assert.AreEqual(feeCharge.Id, reloadedUnits[1].FeeCharges.Single().Id);
        }
    }

    [TestMethod]
    public void SaveChanges_WhenUnitWithSpecificFeeChargeIsDeleted_RemovesUnitRelationshipButKeepsFeeCharge()
    {
        // Arrange
        var (context, connection) = SqliteTestDbContextFactory.Create();
        using (connection)
        using (context)
        {
            var unit = Unit.Create("101", "Address 1", true, "creator").Value;
            context.Units.Add(unit);
            context.SaveChanges();

            var feeCharge = FeeCharge.Create(50m, new DateOnly(2024, 2, 1), "Monthly Fee", ChargeCategory.CondoFee, ChargeScope.SpecificUnit, unit, "creator").Value;
            context.FeeCharges.Add(feeCharge);
            context.SaveChanges();

            // Act
            context.Units.Remove(unit);
            context.SaveChanges();

            // Assert
            var reloadedFeeCharge = context.FeeCharges
                .Include(fc => fc.Units)
                .Single(fc => fc.Id == feeCharge.Id);
            Assert.IsEmpty(reloadedFeeCharge.Units);
        }
    }

    [TestMethod]
    public void Remove_WhenUnitWithPaymentIsDeleted_ThrowsInvalidOperationException()
    {
        // Arrange
        var (context, connection) = SqliteTestDbContextFactory.Create();
        using (connection)
        using (context)
        {
            var unit = Unit.Create("101", "Address 1", true, "creator").Value;
            context.Units.Add(unit);
            context.SaveChanges();

            var payment = Payment.Create(new DateOnly(2024, 1, 15), unit, 100m, PaymentMethod.Cash, "REF1", null, "creator").Value;
            context.Payments.Add(payment);
            context.SaveChanges();

            // Act & Assert
            Assert.ThrowsExactly<InvalidOperationException>(() => context.Units.Remove(unit));
        }
    }

    [TestMethod]
    public void Remove_WhenUnitWithUnitOwnerIsDeleted_ThrowsInvalidOperationException()
    {
        // Arrange
        var (context, connection) = SqliteTestDbContextFactory.Create();
        using (connection)
        using (context)
        {
            var unit = Unit.Create("101", "Address 1", true, "creator").Value;
            var owner = Owner.Create("Owner", null, null, "creator").Value;
            context.Units.Add(unit);
            context.Owners.Add(owner);
            context.SaveChanges();

            var unitOwner = new UnitOwner(unit, owner, new DateOnly(2024, 1, 1), null, DateTime.UtcNow, "creator");
            context.UnitOwners.Add(unitOwner);
            context.SaveChanges();

            // Act & Assert
            Assert.ThrowsExactly<InvalidOperationException>(() => context.Units.Remove(unit));
        }
    }

    [TestMethod]
    public void SaveChanges_RoundTripsAllEntities_ForSavedValuesToMatch()
    {
        // Arrange
        var (context, connection) = SqliteTestDbContextFactory.Create();
        using (connection)
        using (context)
        {
            var unit = Unit.Create("101", "Some Address", true, "creator").Value;
            var owner = Owner.Create("Owner", null, null, "creator").Value;

            context.Units.Add(unit);
            context.Owners.Add(owner);
            context.SaveChanges();

            var unitOwner = new UnitOwner(unit, owner, new DateOnly(2024, 1, 1), null, DateTime.UtcNow, "creator");
            var payment = Payment.Create(new DateOnly(2024, 1, 15), unit, 100m, PaymentMethod.Cash, "REF1", "Some notes", "creator").Value;
            var feeCharge = FeeCharge.Create(50m, new DateOnly(2024, 2, 1), "Monthly Fee", ChargeCategory.CondoFee, ChargeScope.SpecificUnit, unit, "creator").Value;

            context.UnitOwners.Add(unitOwner);
            context.Payments.Add(payment);
            context.FeeCharges.Add(feeCharge);

            // Act
            context.SaveChanges();

            // Assert
            var reloadedUnit = context.Units.Single(u => u.Id == unit.Id);
            var reloadedOwner = context.Owners.Single(o => o.Id == owner.Id);
            var reloadedUnitOwner = context.UnitOwners.Single(uo => uo.Id == unitOwner.Id);
            var reloadedPayment = context.Payments.Single(p => p.Id == payment.Id);
            var reloadedFeeCharge = context.FeeCharges.Single(fc => fc.Id == feeCharge.Id);

            Assert.AreEqual("101", reloadedUnit.UnitNumber);
            Assert.AreEqual("Owner", reloadedOwner.Name);
            Assert.AreEqual(owner.Id, reloadedUnitOwner.OwnerId);
            Assert.AreEqual(unit.Id, reloadedUnitOwner.UnitId);
            Assert.AreEqual(100m, reloadedPayment.Amount);
            Assert.AreEqual(PaymentMethod.Cash, reloadedPayment.Method);
            Assert.AreEqual(50m, reloadedFeeCharge.Amount);
            Assert.AreEqual(ChargeCategory.CondoFee, reloadedFeeCharge.Category);
        }
    }
}
