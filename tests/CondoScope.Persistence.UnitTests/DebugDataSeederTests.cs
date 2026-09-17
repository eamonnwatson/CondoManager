using CondoScope.Domain.Entities;
using CondoScope.Domain.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CondoScope.Persistence.UnitTests;

[TestClass]
public class DebugDataSeederTests
{
    private SqliteConnection? _connection;

    [TestCleanup]
    public void Cleanup()
    {
        _connection?.Dispose();
        _connection = null;
    }

    private AppDbContext CreateContext()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    [TestMethod]
    public async Task SeedAsync_WhenUnitsTableEmpty_SeedsUnitsOwnersPaymentsAndFeeCharges()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        await DebugDataSeeder.SeedAsync(context);

        // Assert
        var unitCount = await context.Units.CountAsync();
        var ownerCount = await context.Owners.CountAsync();
        var paymentCount = await context.Payments.CountAsync();
        var feeChargeCount = await context.FeeCharges.CountAsync();

        Assert.AreEqual(10, unitCount);
        Assert.AreEqual(10, ownerCount);
        Assert.AreEqual(30, paymentCount);
        // 1 all-units condo fee + 1 all-units reserve fee + (2 per unit * 10 units) specific fees
        Assert.AreEqual(22, feeChargeCount);
    }

    [TestMethod]
    public async Task SeedAsync_WhenUnitsAlreadyExist_DoesNotSeedAdditionalData()
    {
        // Arrange
        using var context = CreateContext();
        var existingUnit = Unit.Create("EXISTING-001", "1 Pre-Seeded Ave", true, "test-setup").Value;
        context.Units.Add(existingUnit);
        await context.SaveChangesAsync();

        // Act
        await DebugDataSeeder.SeedAsync(context);

        // Assert
        var unitCount = await context.Units.CountAsync();
        var ownerCount = await context.Owners.CountAsync();
        var paymentCount = await context.Payments.CountAsync();
        var feeChargeCount = await context.FeeCharges.CountAsync();

        Assert.AreEqual(1, unitCount);
        Assert.AreEqual(0, ownerCount);
        Assert.AreEqual(0, paymentCount);
        Assert.AreEqual(0, feeChargeCount);
    }

    [TestMethod]
    public async Task SeedAsync_WhenUnitsTableEmpty_AssignsOwnersToUnitsWithIncrementingStartDates()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        await DebugDataSeeder.SeedAsync(context);

        // Assert
        var units = await context.Units
            .Include(u => u.UnitOwners)
            .OrderBy(u => u.UnitNumber)
            .ToListAsync();

        Assert.AreEqual(10, units.Count);
        foreach (var unit in units)
        {
            Assert.AreEqual(1, unit.UnitOwners.Count);
        }
    }

    [TestMethod]
    public async Task SeedAsync_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        using var context = CreateContext();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(
            () => DebugDataSeeder.SeedAsync(context, cts.Token));

        var unitCount = await context.Units.CountAsync();
        Assert.AreEqual(0, unitCount);
    }

    [TestMethod]
    public async Task SeedAsync_WhenUnitsTableEmpty_CreatesFeeChargesWithCorrectCategoriesAndScopes()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        await DebugDataSeeder.SeedAsync(context);

        // Assert
        var condoFeeCount = await context.FeeCharges
            .CountAsync(f => f.Category == ChargeCategory.CondoFee && f.Scope == ChargeScope.AllUnits);
        var reserveFeeCount = await context.FeeCharges
            .CountAsync(f => f.Category == ChargeCategory.ReserveFee && f.Scope == ChargeScope.AllUnits);
        var otherFeeCount = await context.FeeCharges
            .CountAsync(f => f.Category == ChargeCategory.OtherFee && f.Scope == ChargeScope.SpecificUnit);

        Assert.AreEqual(1, condoFeeCount);
        Assert.AreEqual(1, reserveFeeCount);
        Assert.AreEqual(20, otherFeeCount);
    }
}
