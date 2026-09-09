using CondoScope.Domain.Entities;
using CondoScope.Domain.ValueObjects;

namespace CondoScope.Domain.UnitTests.Entities;

[TestClass]
public class OwnerTests
{
    [TestMethod]
    public void Create_WithValidData_ReturnsSuccessResult()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;
        var phone = PhoneNumber.Create("1234567890").Value;

        // Act
        var result = Owner.Create("John Doe", email, phone, "creator");

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Value);
    }

    [TestMethod]
    public void Create_WithValidData_SetsAllProperties()
    {
        // Arrange
        var email = Email.Create("owner@example.com").Value;
        var phone = PhoneNumber.Create("5551234567").Value;
        const string name = "Jane Smith";
        const string createdBy = "admin";

        // Act
        var result = Owner.Create(name, email, phone, createdBy);

        // Assert
        var owner = result.Value;
        Assert.AreEqual(name, owner.Name);
        Assert.AreEqual(email, owner.Email);
        Assert.AreEqual(phone, owner.Phone);
        Assert.AreEqual(createdBy, owner.CreatedBy);
    }

    [TestMethod]
    public void Create_SetsCreatedAtUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var result = Owner.Create("Name", null, null, "creator");
        var after = DateTime.UtcNow;

        // Assert
        var owner = result.Value;
        Assert.IsTrue(owner.CreatedAtUtc >= before && owner.CreatedAtUtc <= after);
    }

    [TestMethod]
    public void Create_GeneratesNonEmptyId()
    {
        // Act
        var result = Owner.Create("Name", null, null, "creator");

        // Assert
        Assert.AreNotEqual(default, result.Value.Id);
    }

    [TestMethod]
    public void Create_WithNullEmailAndPhone_AllowsNulls()
    {
        // Act
        var result = Owner.Create("Name", null, null, "creator");

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsNull(result.Value.Email);
        Assert.IsNull(result.Value.Phone);
    }

    [TestMethod]
    public void UnitOwners_WhenCreated_IsEmpty()
    {
        // Act
        var result = Owner.Create("Name", null, null, "creator");

        // Assert
        Assert.IsNotNull(result.Value.UnitOwners);
        Assert.IsEmpty(result.Value.UnitOwners);
    }

    [TestMethod]
    public void UnitOwners_ReturnsSameInstanceType_IsReadOnlyCollection()
    {
        // Arrange
        var result = Owner.Create("Name", null, null, "creator");

        // Act
        var unitOwners = result.Value.UnitOwners;

        // Assert
        Assert.IsInstanceOfType<IReadOnlyCollection<UnitOwner>>(unitOwners);
    }
}
