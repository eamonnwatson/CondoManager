using CondoScope.Domain.Entities;
using CondoScope.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CondoScope.Persistence.UnitTests.Configurations;

[TestClass]
public class UnitConfigurationTests
{
    private static IEntityType BuildEntityType()
    {
        var modelBuilder = new ModelBuilder();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(UnitConfiguration).Assembly);
        return modelBuilder.FinalizeModel().FindEntityType(typeof(Unit))!;
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
        Assert.AreEqual(nameof(Unit.Id), primaryKey.Properties[0].Name);
    }

    [TestMethod]
    public void Configure_IdProperty_HasValueConverter()
    {
        // Act
        var entityType = BuildEntityType();

        // Assert
        var idProperty = entityType.FindProperty(nameof(Unit.Id));
        Assert.IsNotNull(idProperty);
        Assert.IsNotNull(idProperty.GetValueConverter());
    }

    [TestMethod]
    public void Configure_IdProperty_ConverterRoundTripsUlid()
    {
        // Arrange
        var entityType = BuildEntityType();
        var idProperty = entityType.FindProperty(nameof(Unit.Id))!;
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
    public void Configure_UnitNumberProperty_HasMaxLengthAndIsRequired()
    {
        // Act
        var entityType = BuildEntityType();

        // Assert
        var unitNumberProperty = entityType.FindProperty(nameof(Unit.UnitNumber));
        Assert.IsNotNull(unitNumberProperty);
        Assert.AreEqual(30, unitNumberProperty.GetMaxLength());
        Assert.IsFalse(unitNumberProperty.IsNullable);
    }

    [TestMethod]
    public void Configure_AddressProperty_HasMaxLengthAndIsRequired()
    {
        // Act
        var entityType = BuildEntityType();

        // Assert
        var addressProperty = entityType.FindProperty(nameof(Unit.Address));
        Assert.IsNotNull(addressProperty);
        Assert.AreEqual(300, addressProperty.GetMaxLength());
        Assert.IsFalse(addressProperty.IsNullable);
    }

    [TestMethod]
    public void Configure_IsActiveProperty_IsRequired()
    {
        // Act
        var entityType = BuildEntityType();

        // Assert
        var isActiveProperty = entityType.FindProperty(nameof(Unit.IsActive));
        Assert.IsNotNull(isActiveProperty);
        Assert.IsFalse(isActiveProperty.IsNullable);
    }

    [TestMethod]
    public void Configure_UnitOwnersNavigation_ConfiguresForeignKeyWithRestrictDeleteBehavior()
    {
        // Act
        var entityType = BuildEntityType();

        // Assert
        var unitOwnersNavigation = entityType.FindNavigation(nameof(Unit.UnitOwners));
        Assert.IsNotNull(unitOwnersNavigation);

        var foreignKey = unitOwnersNavigation.ForeignKey;
        Assert.AreEqual(nameof(UnitOwner.UnitId), foreignKey.Properties[0].Name);
        Assert.AreEqual(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
        Assert.AreEqual(nameof(UnitOwner.Unit), foreignKey.DependentToPrincipal!.Name);
    }

    [TestMethod]
    public void Configure_UnitOwnersNavigation_UsesFieldAccessMode()
    {
        // Act
        var entityType = BuildEntityType();

        // Assert
        var unitOwnersNavigation = entityType.FindNavigation(nameof(Unit.UnitOwners));
        Assert.IsNotNull(unitOwnersNavigation);
        Assert.AreEqual(PropertyAccessMode.Field, unitOwnersNavigation.GetPropertyAccessMode());
    }

    [TestMethod]
    public void Configure_PaymentsNavigation_UsesFieldAccessMode()
    {
        // Act
        var entityType = BuildEntityType();

        // Assert
        var paymentsNavigation = entityType.FindNavigation(nameof(Unit.Payments));
        Assert.IsNotNull(paymentsNavigation);
        Assert.AreEqual(PropertyAccessMode.Field, paymentsNavigation.GetPropertyAccessMode());
    }

    [TestMethod]
    public void Configure_FeeChargesNavigation_UsesFieldAccessMode()
    {
        // Act
        var entityType = BuildEntityType();

        // Assert
        var feeChargesNavigation = entityType.FindSkipNavigation(nameof(Unit.FeeCharges));
        Assert.IsNotNull(feeChargesNavigation);
        Assert.AreEqual(PropertyAccessMode.Field, feeChargesNavigation.GetPropertyAccessMode());
    }
}
