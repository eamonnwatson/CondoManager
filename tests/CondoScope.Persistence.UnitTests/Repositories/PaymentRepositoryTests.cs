using CondoScope.Application.Common.Errors;
using CondoScope.Domain.Entities;
using CondoScope.Domain.Enums;
using CondoScope.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CondoScope.Persistence.UnitTests.Repositories;

[TestClass]
public class PaymentRepositoryTests
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
    public async Task AddAsync_WhenPaymentIsValid_PersistsPaymentAndReturnsSuccess()
    {
        // Arrange
        var (context, connection) = CreateContext();
        using (connection)
        using (context)
        {
            var unit = Unit.Create("101", "Address 1", true, "creator").Value;
            context.Units.Add(unit);
            context.SaveChanges();

            var repository = new PaymentRepository(context);
            var payment = Payment.Create(new DateOnly(2024, 1, 1), unit, 100m, PaymentMethod.Cash, "REF1", "notes", "creator").Value;

            // Act
            var result = await repository.AddAsync(payment, CancellationToken.None);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(payment, result.Value);
            var persisted = context.Payments.Single(p => p.Id == payment.Id);
            Assert.AreEqual(100m, persisted.Amount);
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
            var unit = Unit.Create("101", "Address 1", true, "creator").Value;
            context.Units.Add(unit);
            context.SaveChanges();

            var repository = new PaymentRepository(context);
            var payment = Payment.Create(new DateOnly(2024, 1, 1), unit, 100m, PaymentMethod.Cash, "REF1", "notes", "creator").Value;

            // Act: add the same payment twice to force a SQLite constraint violation on save.
            await repository.AddAsync(payment, CancellationToken.None);
            var result = await repository.AddAsync(payment, CancellationToken.None);

            // Assert
            Assert.IsTrue(result.IsFailed);
            Assert.IsTrue(result.Errors.OfType<DatabaseError>().Any());
        }
    }

    [TestMethod]
    public async Task GetAllAsync_WhenPaymentsExist_ReturnsPaymentsWithUnitLoaded()
    {
        // Arrange
        var (context, connection) = CreateContext();
        using (connection)
        using (context)
        {
            var unit = Unit.Create("101", "Address 1", true, "creator").Value;
            context.Units.Add(unit);
            context.SaveChanges();

            var payment = Payment.Create(new DateOnly(2024, 1, 1), unit, 100m, PaymentMethod.Cash, "REF1", "notes", "creator").Value;
            context.Payments.Add(payment);
            context.SaveChanges();

            var repository = new PaymentRepository(context);

            // Act
            var result = await repository.GetAllAsync(CancellationToken.None);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.HasCount(1, result.Value);
            var reloadedPayment = result.Value[0];
            Assert.AreEqual(payment.Id, reloadedPayment.Id);
            Assert.IsNotNull(reloadedPayment.Unit);
            Assert.AreEqual(unit.Id, reloadedPayment.Unit.Id);
        }
    }

    [TestMethod]
    public async Task GetAllAsync_WhenNoPaymentsExist_ReturnsEmptyList()
    {
        // Arrange
        var (context, connection) = CreateContext();
        using (connection)
        using (context)
        {
            var repository = new PaymentRepository(context);

            // Act
            var result = await repository.GetAllAsync(CancellationToken.None);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsEmpty(result.Value);
        }
    }
}
