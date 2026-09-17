using CondoScope.Application.Common.Errors;
using CondoScope.Domain.Entities;
using CondoScope.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CondoScope.Persistence.UnitTests.Repositories;

[TestClass]
public class OwnerRepositoryTests
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
    public async Task AddAsync_WhenOwnerIsValid_PersistsOwnerAndReturnsSuccess()
    {
        // Arrange
        var (context, connection) = CreateContext();
        using (connection)
        using (context)
        {
            var repository = new OwnerRepository(context);
            var owner = Owner.Create("Owner Name", null, null, "creator").Value;

            // Act
            var result = await repository.AddAsync(owner, CancellationToken.None);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(owner, result.Value);
            var persisted = context.Owners.Single(o => o.Id == owner.Id);
            Assert.AreEqual("Owner Name", persisted.Name);
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
            var repository = new OwnerRepository(context);
            var owner = Owner.Create("Owner Name", null, null, "creator").Value;

            // Act: add the same owner twice to force a SQLite constraint violation on save.
            await repository.AddAsync(owner, CancellationToken.None);
            var result = await repository.AddAsync(owner, CancellationToken.None);

            // Assert
            Assert.IsTrue(result.IsFailed);
            Assert.IsTrue(result.Errors.OfType<DatabaseError>().Any());
        }
    }

    [TestMethod]
    public async Task GetAllAsync_WhenOwnersExistWithUnitOwners_ReturnsOwnersWithUnitOwnerAndUnitLoaded()
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

            var repository = new OwnerRepository(context);

            // Act
            var result = await repository.GetAllAsync(CancellationToken.None);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.HasCount(1, result.Value);
            var reloadedOwner = result.Value[0];
            Assert.AreEqual(owner.Id, reloadedOwner.Id);
            Assert.IsNotNull(reloadedOwner.UnitOwner);
            Assert.AreEqual(unit.Id, reloadedOwner.UnitOwner!.Unit.Id);
        }
    }

    [TestMethod]
    public async Task GetAllAsync_WhenNoOwnersExist_ReturnsEmptyList()
    {
        // Arrange
        var (context, connection) = CreateContext();
        using (connection)
        using (context)
        {
            var repository = new OwnerRepository(context);

            // Act
            var result = await repository.GetAllAsync(CancellationToken.None);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsEmpty(result.Value);
        }
    }
}
