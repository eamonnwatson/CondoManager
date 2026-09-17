using CondoScope.Domain.Entities;
using CondoScope.Domain.ValueObjects;
using CondoScope.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CondoScope.Persistence.UnitTests.Configurations;

[TestClass]
public class OwnerConfigurationTests
{
    private static IEntityType BuildEntityType()
    {
        var modelBuilder = new ModelBuilder();
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OwnerConfiguration).Assembly);
        return modelBuilder.FinalizeModel().FindEntityType(typeof(Owner))!;
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
        Assert.AreEqual(nameof(Owner.Id), primaryKey.Properties[0].Name);
    }

    [TestMethod]
    public void Configure_IdProperty_ConverterRoundTripsUlid()
    {
        // Arrange
        var entityType = BuildEntityType();
        var idProperty = entityType.FindProperty(nameof(Owner.Id))!;
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
    public void Configure_NameProperty_HasMaxLengthAndIsRequired()
    {
        // Act
        var entityType = BuildEntityType();

        // Assert
        var nameProperty = entityType.FindProperty(nameof(Owner.Name));
        Assert.IsNotNull(nameProperty);
        Assert.AreEqual(200, nameProperty.GetMaxLength());
        Assert.IsFalse(nameProperty.IsNullable);
    }

    [TestMethod]
    public void Configure_EmailProperty_HasMaxLength()
    {
        // Act
        var entityType = BuildEntityType();

        // Assert
        var emailProperty = entityType.FindProperty(nameof(Owner.Email));
        Assert.IsNotNull(emailProperty);
        Assert.AreEqual(200, emailProperty.GetMaxLength());
    }

    [TestMethod]
    public void Configure_EmailProperty_ConverterRoundTripsNonNullEmail()
    {
        // Arrange
        var entityType = BuildEntityType();
        var emailProperty = entityType.FindProperty(nameof(Owner.Email))!;
        var converter = emailProperty.GetValueConverter()!;
        var email = Email.Create("owner@example.com").Value;

        // Act
        var providerValue = converter.ConvertToProvider(email);
        var modelValue = converter.ConvertFromProvider(providerValue);

        // Assert
        Assert.AreEqual("owner@example.com", providerValue);
        Assert.AreEqual(email, modelValue);
    }

    [TestMethod]
    public void Configure_EmailProperty_ConverterRoundTripsNullEmail()
    {
        // Arrange
        var entityType = BuildEntityType();
        var emailProperty = entityType.FindProperty(nameof(Owner.Email))!;
        var converter = emailProperty.GetValueConverter()!;

        // Act
        var providerValue = converter.ConvertToProvider(null);
        var modelValue = converter.ConvertFromProvider(null);

        // Assert
        Assert.IsNull(providerValue);
        Assert.IsNull(modelValue);
    }

    [TestMethod]
    public void Configure_PhoneProperty_HasMaxLength()
    {
        // Act
        var entityType = BuildEntityType();

        // Assert
        var phoneProperty = entityType.FindProperty(nameof(Owner.Phone));
        Assert.IsNotNull(phoneProperty);
        Assert.AreEqual(30, phoneProperty.GetMaxLength());
    }

    [TestMethod]
    public void Configure_PhoneProperty_ConverterRoundTripsNonNullPhone()
    {
        // Arrange
        var entityType = BuildEntityType();
        var phoneProperty = entityType.FindProperty(nameof(Owner.Phone))!;
        var converter = phoneProperty.GetValueConverter()!;
        var phone = PhoneNumber.Create("+15145551234").Value;

        // Act
        var providerValue = converter.ConvertToProvider(phone);
        var modelValue = converter.ConvertFromProvider(providerValue);

        // Assert
        Assert.AreEqual(phone.Value, providerValue);
        Assert.AreEqual(phone, modelValue);
    }

    [TestMethod]
    public void Configure_PhoneProperty_ConverterRoundTripsNullPhone()
    {
        // Arrange
        var entityType = BuildEntityType();
        var phoneProperty = entityType.FindProperty(nameof(Owner.Phone))!;
        var converter = phoneProperty.GetValueConverter()!;

        // Act
        var providerValue = converter.ConvertToProvider(null);
        var modelValue = converter.ConvertFromProvider(null);

        // Assert
        Assert.IsNull(providerValue);
        Assert.IsNull(modelValue);
    }

    [TestMethod]
    public void Configure_UnitOwnerNavigation_ConfiguresForeignKeyWithRestrictDeleteBehavior()
    {
        // Act
        var entityType = BuildEntityType();

        // Assert
        var unitOwnerNavigation = entityType.FindNavigation(nameof(Owner.UnitOwner));
        Assert.IsNotNull(unitOwnerNavigation);

        var foreignKey = unitOwnerNavigation.ForeignKey;
        Assert.AreEqual(nameof(UnitOwner.OwnerId), foreignKey.Properties[0].Name);
        Assert.AreEqual(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
        Assert.AreEqual(nameof(UnitOwner.Owner), foreignKey.DependentToPrincipal!.Name);
        Assert.AreEqual(nameof(Owner.UnitOwner), foreignKey.PrincipalToDependent!.Name);
        Assert.IsTrue(foreignKey.IsUnique);
    }
}
