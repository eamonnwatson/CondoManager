using CondoScope.Application.Common.Errors;
using CondoScope.Domain.Entities;
using CondoScope.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CondoScope.Persistence.UnitTests.Repositories;

[TestClass]
public class UnitRepositoryTests
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
    public async Task AddAsync_WhenUnitIsValid_PersistsUnitAndReturnsSuccess()
    {
        // Arrange
        var (context, connection) = CreateContext();
        using (connection)
        using (context)
        {
            var repository = new UnitRepository(context);
            var unit = Unit.Create("101", "Address 1", true, "creator").Value;

            // Act
            var result = await repository.AddAsync(unit, CancellationToken.None);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(unit, result.Value);
            var persisted = context.Units.Single(u => u.Id == unit.Id);
            Assert.AreEqual("101", persisted.UnitNumber);
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
            var repository = new UnitRepository(context);
            var unit = Unit.Create("101", "Address 1", true, "creator").Value;

            // Act: add the same unit twice to force a SQLite constraint violation on save.
            await repository.AddAsync(unit, CancellationToken.None);
            var result = await repository.AddAsync(unit, CancellationToken.None);

            // Assert
            Assert.IsTrue(result.IsFailed);
            Assert.IsTrue(result.Errors.OfType<DatabaseError>().Any());
        }
    }

    [TestMethod]
    public async Task GetAllWithCurrentOwnerAsync_WhenUnitsExist_ReturnsUnitsOrderedByUnitNumberWithCurrentOwner()
    {
        // Arrange
        var (context, connection) = CreateContext();
        using (connection)
        using (context)
        {
            var unit1 = Unit.Create("102", "Address 2", true, "creator").Value;
            var unit2 = Unit.Create("101", "Address 1", true, "creator").Value;
            var owner = Owner.Create("Owner", null, null, "creator").Value;

            context.Units.AddRange(unit1, unit2);
            context.Owners.Add(owner);
            context.SaveChanges();

            var unitOwner = new UnitOwner(unit2, owner, new DateOnly(2024, 1, 1), null, DateTime.UtcNow, "creator");
            context.UnitOwners.Add(unitOwner);
            context.SaveChanges();

            var repository = new UnitRepository(context);

            // Act
            var result = await repository.GetAllWithCurrentOwnerAsync(CancellationToken.None);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.HasCount(2, result.Value);
            Assert.AreEqual("101", result.Value[0].UnitNumber);
            Assert.AreEqual("102", result.Value[1].UnitNumber);
            Assert.IsNotNull(result.Value[0].CurrentOwner);
            Assert.AreEqual(owner.Id, result.Value[0].CurrentOwner!.Id);
            Assert.IsNull(result.Value[1].CurrentOwner);
        }
    }

    [TestMethod]
    public async Task GetAllWithCurrentOwnerAsync_WhenNoUnitsExist_ReturnsEmptyList()
    {
        // Arrange
        var (context, connection) = CreateContext();
        using (connection)
        using (context)
        {
            var repository = new UnitRepository(context);

            // Act
            var result = await repository.GetAllWithCurrentOwnerAsync(CancellationToken.None);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsEmpty(result.Value);
        }
    }

    [TestMethod]
    public async Task GetAllWithDetailsAsync_WhenUnitHasOwnerPaymentsAndFeeCharges_ReturnsUnitWithAllNavigationsLoaded()
    {
        // Arrange
        var (context, connection) = CreateContext();
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

            var repository = new UnitRepository(context);

            // Act
            var result = await repository.GetAllWithDetailsAsync(CancellationToken.None);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.HasCount(1, result.Value);
            var reloadedUnit = result.Value[0];
            Assert.HasCount(1, reloadedUnit.UnitOwners);
            Assert.AreEqual(owner.Id, reloadedUnit.UnitOwners.First().Owner.Id);
        }
    }

    [TestMethod]
    public async Task GetAllWithDetailsAsync_WhenNoUnitsExist_ReturnsEmptyList()
    {
        // Arrange
        var (context, connection) = CreateContext();
        using (connection)
        using (context)
        {
            var repository = new UnitRepository(context);

            // Act
            var result = await repository.GetAllWithDetailsAsync(CancellationToken.None);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsEmpty(result.Value);
        }
    }

    [TestMethod]
    public async Task GetByIdWithDetailsAsync_WhenUnitExists_ReturnsSuccessWithUnit()
    {
        // Arrange
        var (context, connection) = CreateContext();
        using (connection)
        using (context)
        {
            var unit = Unit.Create("101", "Address 1", true, "creator").Value;
            context.Units.Add(unit);
            context.SaveChanges();

            var repository = new UnitRepository(context);

            // Act
            var result = await repository.GetByIdWithDetailsAsync(unit.Id, CancellationToken.None);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(unit.Id, result.Value.Id);
        }
    }

    [TestMethod]
    public async Task GetByIdWithDetailsAsync_WhenUnitDoesNotExist_ReturnsFailedResultWithNotFoundError()
    {
        // Arrange
        var (context, connection) = CreateContext();
        using (connection)
        using (context)
        {
            var repository = new UnitRepository(context);
            var missingId = Ulid.NewUlid();

            // Act
            var result = await repository.GetByIdWithDetailsAsync(missingId, CancellationToken.None);

            // Assert
            Assert.IsTrue(result.IsFailed);
            var notFoundError = result.Errors.OfType<NotFoundError>().Single();
            Assert.AreEqual($"Unit with ID {missingId} not found.", notFoundError.Message);
        }
    }

    [TestMethod]
    public async Task GetUnitsWithNoOwnersAsync_WhenSomeUnitsHaveOwnersAndSomeDoNot_ReturnsOnlyUnitsWithoutOwners()
    {
        // Arrange
        var (context, connection) = CreateContext();
        using (connection)
        using (context)
        {
            var unitWithOwner = Unit.Create("102", "Address 2", true, "creator").Value;
            var unitWithoutOwner = Unit.Create("101", "Address 1", true, "creator").Value;
            var owner = Owner.Create("Owner", null, null, "creator").Value;

            context.Units.AddRange(unitWithOwner, unitWithoutOwner);
            context.Owners.Add(owner);
            context.SaveChanges();

            var unitOwner = new UnitOwner(unitWithOwner, owner, new DateOnly(2024, 1, 1), null, DateTime.UtcNow, "creator");
            context.UnitOwners.Add(unitOwner);
            context.SaveChanges();

            var repository = new UnitRepository(context);

            // Act
            var result = await repository.GetUnitsWithNoOwnersAsync(CancellationToken.None);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.HasCount(1, result.Value);
            Assert.AreEqual(unitWithoutOwner.Id, result.Value[0].Id);
        }
    }

    [TestMethod]
    public async Task GetUnitsWithNoOwnersAsync_WhenAllUnitsHaveOwners_ReturnsEmptyList()
    {
        // Arrange
        var (context, connection) = CreateContext();
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

            var repository = new UnitRepository(context);

            // Act
            var result = await repository.GetUnitsWithNoOwnersAsync(CancellationToken.None);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsEmpty(result.Value);
        }
    }
}
