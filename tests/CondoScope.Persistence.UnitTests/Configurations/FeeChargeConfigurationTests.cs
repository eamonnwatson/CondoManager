using CondoScope.Domain.Entities;
using CondoScope.Domain.Enums;
using CondoScope.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CondoScope.Persistence.UnitTests.Configurations;

[TestClass]
public class FeeChargeConfigurationTests
{
    private static IEntityType BuildEntityType()
    {
        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<Unit>();
        modelBuilder.Entity<FeeChargeUnit>();
        new FeeChargeConfiguration().Configure(modelBuilder.Entity<FeeCharge>());
        return modelBuilder.FinalizeModel().FindEntityType(typeof(FeeCharge))!;
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
        Assert.AreEqual(nameof(FeeCharge.Id), primaryKey.Properties[0].Name);
    }

    [TestMethod]
    public void Configure_IdProperty_ConverterRoundTripsUlid()
    {
        // Arrange
        var entityType = BuildEntityType();
        var idProperty = entityType.FindProperty(nameof(FeeCharge.Id))!;
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
    public void Configure_DueDateProperty_IsRequired()
    {
        // Act
        var entityType = BuildEntityType();

        // Assert
        var dueDateProperty = entityType.FindProperty(nameof(FeeCharge.DueDate));
        Assert.IsNotNull(dueDateProperty);
        Assert.IsFalse(dueDateProperty.IsNullable);
    }

    [TestMethod]
    public void Configure_DescriptionProperty_HasMaxLengthAndIsRequired()
    {
        // Act
        var entityType = BuildEntityType();

        // Assert
        var descriptionProperty = entityType.FindProperty(nameof(FeeCharge.Description));
        Assert.IsNotNull(descriptionProperty);
        Assert.IsFalse(descriptionProperty.IsNullable);
        Assert.AreEqual(500, descriptionProperty.GetMaxLength());
    }

    [TestMethod]
    public void Configure_CategoryProperty_HasStringConversionAndIsRequired()
    {
        // Act
        var entityType = BuildEntityType();

        // Assert
        var categoryProperty = entityType.FindProperty(nameof(FeeCharge.Category));
        Assert.IsNotNull(categoryProperty);
        Assert.IsFalse(categoryProperty.IsNullable);
        Assert.AreEqual(typeof(string), categoryProperty.GetProviderClrType());
    }

    [TestMethod]
    public void Configure_ScopeProperty_HasStringConversionAndIsRequired()
    {
        // Act
        var entityType = BuildEntityType();

        // Assert
        var scopeProperty = entityType.FindProperty(nameof(FeeCharge.Scope));
        Assert.IsNotNull(scopeProperty);
        Assert.IsFalse(scopeProperty.IsNullable);
        Assert.AreEqual(typeof(string), scopeProperty.GetProviderClrType());
    }

    [TestMethod]
    public void Configure_AmountProperty_HasPrecisionAndIsRequired()
    {
        // Act
        var entityType = BuildEntityType();

        // Assert
        var amountProperty = entityType.FindProperty(nameof(FeeCharge.Amount));
        Assert.IsNotNull(amountProperty);
        Assert.IsFalse(amountProperty.IsNullable);
        Assert.AreEqual(18, amountProperty.GetPrecision());
        Assert.AreEqual(2, amountProperty.GetScale());
    }

    [TestMethod]
    public void Configure_UnitsNavigation_ConfiguresManyToManyThroughFeeChargeUnit()
    {
        // Act
        var entityType = BuildEntityType();

        // Assert
        var unitsNavigation = entityType.FindSkipNavigation(nameof(FeeCharge.Units));
        Assert.IsNotNull(unitsNavigation);
        Assert.AreEqual(typeof(FeeChargeUnit), unitsNavigation.JoinEntityType!.ClrType);
        Assert.AreEqual(nameof(Unit.FeeCharges), unitsNavigation.Inverse.Name);
    }

    [TestMethod]
    public void Configure_UnitsNavigation_UsesFieldAccessMode()
    {
        // Act
        var entityType = BuildEntityType();

        // Assert
        var unitsNavigation = entityType.FindSkipNavigation(nameof(FeeCharge.Units));
        Assert.IsNotNull(unitsNavigation);
        Assert.AreEqual(PropertyAccessMode.Field, unitsNavigation.GetPropertyAccessMode());
    }
}
