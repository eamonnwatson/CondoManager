using CondoScope.Domain.Entities;
using CondoScope.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CondoScope.Persistence.UnitTests.Configurations;

[TestClass]
public class UnitOwnerConfigurationTests
{
    private static IEntityType BuildEntityType()
    {
        var modelBuilder = new ModelBuilder();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UnitOwnerConfiguration).Assembly);
        return modelBuilder.FinalizeModel().FindEntityType(typeof(UnitOwner))!;
    }

    [TestMethod]
    public void Configure_KeyIsId_ConfiguresIdAsPrimaryKey()
    {
        // Act
        var entityType = BuildEntityType();

        // Assert
        var primaryKey = entityType.FindPrimaryKey();
        Assert.IsNotNull(primaryKey);
        Assert.AreEqual(1, primaryKey.Properties.Count);
        Assert.AreEqual(nameof(UnitOwner.Id), primaryKey.Properties[0].Name);
    }

    [TestMethod]
    public void Configure_IdProperty_ConverterRoundTripsUlid()
    {
        // Arrange
        var entityType = BuildEntityType();
        var idProperty = entityType.FindProperty(nameof(UnitOwner.Id))!;
        var converter = idProperty.GetValueConverter()!;
        var ulid = Ulid.NewUlid();

        // Act
        var providerValue = converter.ConvertToProvider(ulid);
        var modelValue = converter.ConvertFromProvider(providerValue);

        // Assert
        Assert.AreEqual(ulid.ToString(), providerValue);
        Assert.AreEqual(ulid, modelValue);
    }

    [TestMethod]
    public void Configure_UnitIdProperty_ConverterRoundTripsUlid()
    {
        // Arrange
        var entityType = BuildEntityType();
        var unitIdProperty = entityType.FindProperty(nameof(UnitOwner.UnitId))!;
        var converter = unitIdProperty.GetValueConverter()!;
        var ulid = Ulid.NewUlid();

        // Act
        var providerValue = converter.ConvertToProvider(ulid);
        var modelValue = converter.ConvertFromProvider(providerValue);

        // Assert
        Assert.AreEqual(ulid.ToString(), providerValue);
        Assert.AreEqual(ulid, modelValue);
    }

    [TestMethod]
    public void Configure_OwnerIdProperty_ConverterRoundTripsUlid()
    {
        // Arrange
        var entityType = BuildEntityType();
        var ownerIdProperty = entityType.FindProperty(nameof(UnitOwner.OwnerId))!;
        var converter = ownerIdProperty.GetValueConverter()!;
        var ulid = Ulid.NewUlid();

        // Act
        var providerValue = converter.ConvertToProvider(ulid);
        var modelValue = converter.ConvertFromProvider(providerValue);

        // Assert
        Assert.AreEqual(ulid.ToString(), providerValue);
        Assert.AreEqual(ulid, modelValue);
    }

    [TestMethod]
    public void Configure_EffectiveFromProperty_IsRequired()
    {
        // Act
        var entityType = BuildEntityType();

        // Assert
        var effectiveFromProperty = entityType.FindProperty(nameof(UnitOwner.EffectiveFrom));
        Assert.IsNotNull(effectiveFromProperty);
        Assert.IsFalse(effectiveFromProperty.IsNullable);
    }

    [TestMethod]
    public void Configure_UnitIdAndEffectiveFromIndex_IsUnique()
    {
        // Act
        var entityType = BuildEntityType();

        // Assert
        var index = entityType.GetIndexes()
            .FirstOrDefault(i => i.Properties.Count == 2
                && i.Properties.Any(p => p.Name == nameof(UnitOwner.UnitId))
                && i.Properties.Any(p => p.Name == nameof(UnitOwner.EffectiveFrom)));
        Assert.IsNotNull(index);
        Assert.IsTrue(index.IsUnique);
    }

    [TestMethod]
    public void Configure_OwnerIdIndex_IsUnique()
    {
        // Act
        var entityType = BuildEntityType();

        // Assert
        var index = entityType.GetIndexes()
            .FirstOrDefault(i => i.Properties.Count == 1
                && i.Properties[0].Name == nameof(UnitOwner.OwnerId));
        Assert.IsNotNull(index);
        Assert.IsTrue(index.IsUnique);
    }

    [TestMethod]
    public void Configure_OwnerNavigation_ConfiguresForeignKeyWithRestrictDeleteBehavior()
    {
        // Act
        var entityType = BuildEntityType();

        // Assert
        var ownerNavigation = entityType.FindNavigation(nameof(UnitOwner.Owner));
        Assert.IsNotNull(ownerNavigation);

        var foreignKey = ownerNavigation.ForeignKey;
        Assert.AreEqual(nameof(UnitOwner.OwnerId), foreignKey.Properties[0].Name);
        Assert.AreEqual(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
        Assert.IsTrue(foreignKey.IsUnique);
        Assert.AreEqual(nameof(Owner.UnitOwner), foreignKey.PrincipalToDependent!.Name);
    }
}
