using CondoScope.Application.Common.Errors;
using CondoScope.Domain.Entities;
using CondoScope.Domain.Enums;
using CondoScope.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CondoScope.Persistence.UnitTests.Repositories;

[TestClass]
public class FeeChargeRepositoryTests
{
    private static (AppDbContext Context, SqliteConnection Connection) CreateContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();

        return (context, connection);
    }

    [TestMethod]
    public async Task AddAsync_WhenFeeChargeIsValid_PersistsFeeChargeAndReturnsSuccess()
    {
        // Arrange
        var (context, connection) = CreateContext();
        using (connection)
        using (context)
        {
            var repository = new FeeChargeRepository(context);
            var feeCharge = FeeCharge.Create(100m, new DateOnly(2024, 1, 1), "Description",
                ChargeCategory.CondoFee, ChargeScope.AllUnits, null, "creator",
                new List<Unit> { Unit.Create("101", "Address 1", true, "creator").Value }).Value;

            // Act
            var result = await repository.AddAsync(feeCharge, CancellationToken.None);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(feeCharge, result.Value);
            var persisted = context.FeeCharges.Single(fc => fc.Id == feeCharge.Id);
            Assert.AreEqual("Description", persisted.Description);
        }
    }

    [TestMethod]
    public async Task AddAsync_WhenSaveChangesThrows_ReturnsFailedResultWithDatabaseError()
    {
        // Arrange
        var (context, connection) = CreateContext();
        using (connection)
        using (context)
        {
            var repository = new FeeChargeRepository(context);
            var feeCharge = FeeCharge.Create(100m, new DateOnly(2024, 1, 1), "Description",
                ChargeCategory.CondoFee, ChargeScope.AllUnits, null, "creator",
                new List<Unit> { Unit.Create("101", "Address 1", true, "creator").Value }).Value;

            // Act: add the same fee charge twice to force a SQLite constraint violation on save.
            await repository.AddAsync(feeCharge, CancellationToken.None);
            var result = await repository.AddAsync(feeCharge, CancellationToken.None);

            // Assert
            Assert.IsTrue(result.IsFailed);
            Assert.IsTrue(result.Errors.OfType<DatabaseError>().Any());
        }
    }

    [TestMethod]
    public async Task GetAllAsync_WhenFeeChargesExist_ReturnsFeeChargesOrderedByDueDateDescendingWithUnits()
    {
        // Arrange
        var (context, connection) = CreateContext();
        using (connection)
        using (context)
        {
            var unit = Unit.Create("101", "Address 1", true, "creator").Value;
            context.Units.Add(unit);
            context.SaveChanges();

            var earlier = FeeCharge.Create(50m, new DateOnly(2024, 1, 1), "Earlier",
                ChargeCategory.CondoFee, ChargeScope.SpecificUnit, unit, "creator").Value;
            var later = FeeCharge.Create(75m, new DateOnly(2024, 6, 1), "Later",
                ChargeCategory.ReserveFee, ChargeScope.SpecificUnit, unit, "creator").Value;

            context.FeeCharges.AddRange(earlier, later);
            context.SaveChanges();

            var repository = new FeeChargeRepository(context);

            // Act
            var result = await repository.GetAllAsync(CancellationToken.None);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.HasCount(2, result.Value);
            Assert.AreEqual(later.Id, result.Value[0].Id);
            Assert.AreEqual(earlier.Id, result.Value[1].Id);
            Assert.HasCount(1, result.Value[0].Units);
            Assert.AreEqual(unit.Id, result.Value[0].Units.First().Id);
        }
    }

    [TestMethod]
    public async Task GetAllAsync_WhenNoFeeChargesExist_ReturnsEmptyList()
    {
        // Arrange
        var (context, connection) = CreateContext();
        using (connection)
        using (context)
        {
            var repository = new FeeChargeRepository(context);

            // Act
            var result = await repository.GetAllAsync(CancellationToken.None);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsEmpty(result.Value);
        }
    }

    [TestMethod]
    public async Task GetAllAsync_WhenCancellationTokenIsAlreadyCanceled_ReturnsFailedResult()
    {
        // Arrange
        var (context, connection) = CreateContext();
        using (connection)
        using (context)
        {
            var repository = new FeeChargeRepository(context);
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            // Act
            var result = await repository.GetAllAsync(cts.Token);

            // Assert
            Assert.IsTrue(result.IsFailed);
        }
    }
}
