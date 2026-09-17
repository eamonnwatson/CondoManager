using CondoScope.Domain.Entities;
using CondoScope.Domain.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CondoScope.Persistence.UnitTests;

[TestClass]
public class AppDbContextTests
{
    private static AppDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [TestMethod]
    public void Owners_WhenAccessed_ReturnsDbSetBackedByOwnerEntity()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        using var context = CreateContext(connection);

        var ownerResult = Owner.Create("John Doe", null, null, "tester");
        Assert.IsTrue(ownerResult.IsSuccess);
        var owner = ownerResult.Value;

        context.Owners.Add(owner);
        context.SaveChanges();

        var retrieved = context.Owners.Find(owner.Id);

        Assert.IsNotNull(retrieved);
        Assert.AreEqual(owner.Name, retrieved!.Name);
    }

    [TestMethod]
    public void Units_WhenAccessed_ReturnsDbSetBackedByUnitEntity()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        using var context = CreateContext(connection);

        var unitResult = Unit.Create("A-101", "123 Main St", true, "tester");
        Assert.IsTrue(unitResult.IsSuccess);
        var unit = unitResult.Value;

        context.Units.Add(unit);
        context.SaveChanges();

        var retrieved = context.Units.Find(unit.Id);

        Assert.IsNotNull(retrieved);
        Assert.AreEqual(unit.UnitNumber, retrieved!.UnitNumber);
    }

    [TestMethod]
    public void UnitOwners_WhenAccessed_ReturnsDbSetBackedByUnitOwnerEntity()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        using var context = CreateContext(connection);

        var ownerResult = Owner.Create("Jane Doe", null, null, "tester");
        var unitResult = Unit.Create("B-202", "456 Main St", true, "tester");
        Assert.IsTrue(ownerResult.IsSuccess);
        Assert.IsTrue(unitResult.IsSuccess);
        var owner = ownerResult.Value;
        var unit = unitResult.Value;

        var assignResult = unit.AssignOwner(owner, DateOnly.FromDateTime(DateTime.UtcNow), "tester");
        Assert.IsTrue(assignResult.IsSuccess);

        context.Owners.Add(owner);
        context.Units.Add(unit);
        context.SaveChanges();

        var retrieved = context.UnitOwners.SingleOrDefault(uo => uo.OwnerId == owner.Id && uo.UnitId == unit.Id);

        Assert.IsNotNull(retrieved);
        Assert.AreEqual(owner.Id, retrieved!.OwnerId);
        Assert.AreEqual(unit.Id, retrieved.UnitId);
    }

    [TestMethod]
    public void FeeCharges_WhenAccessed_ReturnsDbSetBackedByFeeChargeEntity()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        using var context = CreateContext(connection);

        var unitResult = Unit.Create("C-303", "789 Main St", true, "tester");
        Assert.IsTrue(unitResult.IsSuccess);
        var unit = unitResult.Value;

        var feeChargeResult = FeeCharge.Create(
            100.50m,
            DateOnly.FromDateTime(DateTime.UtcNow),
            "Maintenance Fee",
            ChargeCategory.CondoFee,
            ChargeScope.SpecificUnit,
            unit,
            "tester");
        Assert.IsTrue(feeChargeResult.IsSuccess);
        var feeCharge = feeChargeResult.Value;

        context.Units.Add(unit);
        context.FeeCharges.Add(feeCharge);
        context.SaveChanges();

        var retrieved = context.FeeCharges.Find(feeCharge.Id);

        Assert.IsNotNull(retrieved);
        Assert.AreEqual(feeCharge.Description, retrieved!.Description);
        Assert.AreEqual(feeCharge.Amount, retrieved.Amount);
    }

    [TestMethod]
    public void FeeChargeUnits_WhenAccessed_ReturnsDbSetBackedByFeeChargeUnitEntity()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        using var context = CreateContext(connection);

        var unitResult = Unit.Create("D-404", "321 Main St", true, "tester");
        Assert.IsTrue(unitResult.IsSuccess);
        var unit = unitResult.Value;

        var feeChargeResult = FeeCharge.Create(
            50.25m,
            DateOnly.FromDateTime(DateTime.UtcNow),
            "Repair Fee",
            ChargeCategory.OtherFee,
            ChargeScope.SpecificUnit,
            unit,
            "tester");
        Assert.IsTrue(feeChargeResult.IsSuccess);
        var feeCharge = feeChargeResult.Value;

        context.Units.Add(unit);
        context.FeeCharges.Add(feeCharge);
        context.SaveChanges();

        var retrieved = context.FeeChargeUnits
            .SingleOrDefault(fcu => fcu.FeeChargeId == feeCharge.Id && fcu.UnitId == unit.Id);

        Assert.IsNotNull(retrieved);
        Assert.AreEqual(unit.Id, retrieved!.UnitId);
        Assert.AreEqual(feeCharge.Id, retrieved.FeeChargeId);
    }

    [TestMethod]
    public void Payments_WhenAccessed_ReturnsDbSetBackedByPaymentEntity()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        using var context = CreateContext(connection);

        var unitResult = Unit.Create("E-505", "555 Main St", true, "tester");
        Assert.IsTrue(unitResult.IsSuccess);
        var unit = unitResult.Value;

        var paymentResult = Payment.Create(
            DateOnly.FromDateTime(DateTime.UtcNow),
            unit,
            250.00m,
            PaymentMethod.ETransfer,
            "REF-123",
            "Monthly payment",
            "tester");
        Assert.IsTrue(paymentResult.IsSuccess);
        var payment = paymentResult.Value;

        context.Units.Add(unit);
        context.Payments.Add(payment);
        context.SaveChanges();

        var retrieved = context.Payments.Find(payment.Id);

        Assert.IsNotNull(retrieved);
        Assert.AreEqual(payment.Amount, retrieved!.Amount);
        Assert.AreEqual(payment.Reference, retrieved.Reference);
    }

    [TestMethod]
    public void OnModelCreating_WhenContextIsCreated_AppliesEntityConfigurationsFromAssembly()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        using var context = CreateContext(connection);

        var model = context.Model;

        Assert.IsNotNull(model.FindEntityType(typeof(Payment)));
        Assert.IsNotNull(model.FindEntityType(typeof(Owner)));
        Assert.IsNotNull(model.FindEntityType(typeof(Unit)));
        Assert.IsNotNull(model.FindEntityType(typeof(FeeCharge)));
    }
}
