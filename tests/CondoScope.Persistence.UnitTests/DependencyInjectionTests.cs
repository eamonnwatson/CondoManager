using CondoScope.Application.Common.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CondoScope.Persistence.UnitTests;

[TestClass]
public class DependencyInjectionTests
{
    [TestMethod]
    public void AddPersistence_WhenCalled_RegistersAppDbContextAndRepositories()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddPersistence("DataSource=:memory:");

        // Assert
        Assert.AreSame(services, result);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.IsNotNull(scope.ServiceProvider.GetService<AppDbContext>());
        Assert.IsNotNull(scope.ServiceProvider.GetService<IFeeChargeRepository>());
        Assert.IsNotNull(scope.ServiceProvider.GetService<IPaymentRepository>());
        Assert.IsNotNull(scope.ServiceProvider.GetService<IOwnersRepository>());
        Assert.IsNotNull(scope.ServiceProvider.GetService<IUnitsRepository>());
    }

    [TestMethod]
    public void AddPersistence_WhenCalled_RegistersDependenciesAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddPersistence("DataSource=:memory:");

        // Assert
        Assert.IsTrue(services.Any(d => d.ServiceType == typeof(IFeeChargeRepository) && d.Lifetime == ServiceLifetime.Scoped));
        Assert.IsTrue(services.Any(d => d.ServiceType == typeof(IPaymentRepository) && d.Lifetime == ServiceLifetime.Scoped));
        Assert.IsTrue(services.Any(d => d.ServiceType == typeof(IOwnersRepository) && d.Lifetime == ServiceLifetime.Scoped));
        Assert.IsTrue(services.Any(d => d.ServiceType == typeof(IUnitsRepository) && d.Lifetime == ServiceLifetime.Scoped));
    }

    [TestMethod]
    public async Task ApplyPersistenceAsync_WhenCalled_AppliesMigrationsToDatabase()
    {
        // Arrange
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        try
        {
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));
            using var provider = services.BuildServiceProvider();

            // Act
            await provider.ApplyPersistenceAsync(CancellationToken.None);

            // Assert
            using var scope = provider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var appliedMigrations = await dbContext.Database.GetAppliedMigrationsAsync();
            Assert.IsTrue(appliedMigrations.Any());
        }
        finally
        {
            connection.Close();
            connection.Dispose();
        }
    }

    [TestMethod]
    public async Task ApplyPersistenceAsync_WhenCancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        try
        {
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(connection));
            using var provider = services.BuildServiceProvider();

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsExactlyAsync<OperationCanceledException>(
                () => provider.ApplyPersistenceAsync(cts.Token));
        }
        finally
        {
            connection.Close();
            connection.Dispose();
        }
    }
}
