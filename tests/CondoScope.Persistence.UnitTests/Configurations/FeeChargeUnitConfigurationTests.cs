using CondoScope.Domain.Entities;
using CondoScope.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CondoScope.Persistence.UnitTests.Configurations;

[TestClass]
public class FeeChargeUnitConfigurationTests
{
    private sealed class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FeeChargeUnitConfiguration).Assembly);
        }
    }

    private static IEntityType GetEntityType()
    {
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(connection)
            .Options;

        using var context = new TestDbContext(options);
        var entityType = context.Model.FindEntityType(typeof(FeeChargeUnit));

        Assert.IsNotNull(entityType);
        return entityType!;
    }

    [TestMethod]
    public void Configure_PrimaryKey_ComposedOfFeeChargeIdAndUnitId()
    {
        // Arrange
        var entityType = GetEntityType();

        // Act
        var primaryKey = entityType.FindPrimaryKey();

        // Assert
        Assert.IsNotNull(primaryKey);
        var keyPropertyNames = primaryKey!.Properties.Select(p => p.Name).ToArray();
        CollectionAssert.AreEquivalent(
            new[] { nameof(FeeChargeUnit.FeeChargeId), nameof(FeeChargeUnit.UnitId) },
            keyPropertyNames);
    }

    [TestMethod]
    public void Configure_FeeChargeIdProperty_HasStringConversionRoundTrippingUlid()
    {
        // Arrange
        var entityType = GetEntityType();
        var property = entityType.FindProperty(nameof(FeeChargeUnit.FeeChargeId));
        Assert.IsNotNull(property);

        // Act
        var converter = property!.GetValueConverter();

        // Assert
        Assert.IsNotNull(converter);
        Assert.AreEqual(typeof(Ulid), converter!.ModelClrType);
        Assert.AreEqual(typeof(string), converter.ProviderClrType);

        var ulid = Ulid.NewUlid();
        var providerValue = converter.ConvertToProvider(ulid);
        Assert.AreEqual(ulid.ToString(), providerValue);

        var roundTripped = converter.ConvertFromProvider(providerValue);
        Assert.AreEqual(ulid, roundTripped);
    }

    [TestMethod]
    public void Configure_UnitIdProperty_HasStringConversionRoundTrippingUlid()
    {
        // Arrange
        var entityType = GetEntityType();
        var property = entityType.FindProperty(nameof(FeeChargeUnit.UnitId));
        Assert.IsNotNull(property);

        // Act
        var converter = property!.GetValueConverter();

        // Assert
        Assert.IsNotNull(converter);
        Assert.AreEqual(typeof(Ulid), converter!.ModelClrType);
        Assert.AreEqual(typeof(string), converter.ProviderClrType);

        var ulid = Ulid.NewUlid();
        var providerValue = converter.ConvertToProvider(ulid);
        Assert.AreEqual(ulid.ToString(), providerValue);

        var roundTripped = converter.ConvertFromProvider(providerValue);
        Assert.AreEqual(ulid, roundTripped);
    }

    [TestMethod]
    public void Configure_FeeChargeForeignKey_TargetsFeeChargeIdWithCascadeDelete()
    {
        // Arrange
        var entityType = GetEntityType();

        // Act
        var foreignKey = entityType.GetForeignKeys()
            .Single(fk => fk.DependentToPrincipal != null
                && fk.DependentToPrincipal.Name == nameof(FeeChargeUnit.FeeCharge));

        // Assert
        Assert.AreEqual(DeleteBehavior.Cascade, foreignKey.DeleteBehavior);
        var fkPropertyNames = foreignKey.Properties.Select(p => p.Name).ToArray();
        CollectionAssert.AreEquivalent(
            new[] { nameof(FeeChargeUnit.FeeChargeId) },
            fkPropertyNames);
    }

    [TestMethod]
    public void Configure_UnitForeignKey_TargetsUnitIdWithCascadeDelete()
    {
        // Arrange
        var entityType = GetEntityType();

        // Act
        var foreignKey = entityType.GetForeignKeys()
            .Single(fk => fk.DependentToPrincipal != null
                && fk.DependentToPrincipal.Name == nameof(FeeChargeUnit.Unit));

        // Assert
        Assert.AreEqual(DeleteBehavior.Cascade, foreignKey.DeleteBehavior);
        var fkPropertyNames = foreignKey.Properties.Select(p => p.Name).ToArray();
        CollectionAssert.AreEquivalent(
            new[] { nameof(FeeChargeUnit.UnitId) },
            fkPropertyNames);
    }

    [TestMethod]
    public void Configure_Entity_HasNavigationBasedForeignKeysForFeeChargeAndUnit()
    {
        // Arrange
        var entityType = GetEntityType();

        // Act
        var navigationForeignKeyCount = entityType.GetForeignKeys()
            .Count(fk => fk.DependentToPrincipal != null
                && (fk.DependentToPrincipal.Name == nameof(FeeChargeUnit.FeeCharge)
                    || fk.DependentToPrincipal.Name == nameof(FeeChargeUnit.Unit)));

        // Assert
        Assert.AreEqual(2, navigationForeignKeyCount);
    }

}
