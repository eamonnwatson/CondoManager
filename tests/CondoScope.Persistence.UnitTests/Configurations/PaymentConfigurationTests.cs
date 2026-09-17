using CondoScope.Domain.Entities;
using CondoScope.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CondoScope.Persistence.UnitTests.Configurations;

[TestClass]
public class PaymentConfigurationTests
{
    private static IEntityType BuildEntityType()
    {
        var modelBuilder = new ModelBuilder();
        modelBuilder.Entity<Unit>();
        new PaymentConfiguration().Configure(modelBuilder.Entity<Payment>());
        return modelBuilder.FinalizeModel().FindEntityType(typeof(Payment))!;
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
        Assert.AreEqual(nameof(Payment.Id), primaryKey.Properties[0].Name);
    }

    [TestMethod]
    public void Configure_IdProperty_ConverterRoundTripsUlid()
    {
        // Arrange
        var entityType = BuildEntityType();
        var idProperty = entityType.FindProperty(nameof(Payment.Id))!;
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
        var unitIdProperty = entityType.FindProperty(nameof(Payment.UnitId))!;
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
    public void Configure_PaymentDateProperty_IsRequired()
    {
        // Act
        var entityType = BuildEntityType();

        // Assert
        var paymentDateProperty = entityType.FindProperty(nameof(Payment.PaymentDate));
        Assert.IsNotNull(paymentDateProperty);
        Assert.IsFalse(paymentDateProperty.IsNullable);
    }

    [TestMethod]
    public void Configure_AmountProperty_HasPrecisionAndIsRequired()
    {
        // Act
        var entityType = BuildEntityType();

        // Assert
        var amountProperty = entityType.FindProperty(nameof(Payment.Amount));
        Assert.IsNotNull(amountProperty);
        Assert.IsFalse(amountProperty.IsNullable);
        Assert.AreEqual(18, amountProperty.GetPrecision());
        Assert.AreEqual(2, amountProperty.GetScale());
    }

    [TestMethod]
    public void Configure_MethodProperty_HasStringConversionAndIsRequired()
    {
        // Act
        var entityType = BuildEntityType();

        // Assert
        var methodProperty = entityType.FindProperty(nameof(Payment.Method));
        Assert.IsNotNull(methodProperty);
        Assert.IsFalse(methodProperty.IsNullable);
        Assert.AreEqual(typeof(string), methodProperty.GetProviderClrType());
    }

    [TestMethod]
    public void Configure_ReferenceProperty_HasMaxLength()
    {
        // Act
        var entityType = BuildEntityType();

        // Assert
        var referenceProperty = entityType.FindProperty(nameof(Payment.Reference));
        Assert.IsNotNull(referenceProperty);
        Assert.AreEqual(100, referenceProperty.GetMaxLength());
    }

    [TestMethod]
    public void Configure_NotesProperty_HasMaxLength()
    {
        // Act
        var entityType = BuildEntityType();

        // Assert
        var notesProperty = entityType.FindProperty(nameof(Payment.Notes));
        Assert.IsNotNull(notesProperty);
        Assert.AreEqual(1000, notesProperty.GetMaxLength());
    }

    [TestMethod]
    public void Configure_UnitNavigation_ConfiguresForeignKeyWithRestrictDeleteBehavior()
    {
        // Act
        var entityType = BuildEntityType();

        // Assert
        var unitNavigation = entityType.FindNavigation(nameof(Payment.Unit));
        Assert.IsNotNull(unitNavigation);

        var foreignKey = unitNavigation.ForeignKey;
        Assert.AreEqual(nameof(Payment.UnitId), foreignKey.Properties[0].Name);
        Assert.AreEqual(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
        Assert.AreEqual(nameof(Payment.Unit), foreignKey.DependentToPrincipal!.Name);
        Assert.AreEqual(nameof(Unit.Payments), foreignKey.PrincipalToDependent!.Name);
    }
}
