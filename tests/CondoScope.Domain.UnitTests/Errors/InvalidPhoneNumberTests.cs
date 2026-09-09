using CondoScope.Domain.Errors;

namespace CondoScope.Domain.UnitTests.Errors;

[TestClass]
public class InvalidPhoneNumberTests
{
    [TestMethod]
    public void Constructor_ValidPhoneNumber_SetsPhoneNumberProperty()
    {
        // Arrange
        const string phoneNumber = "123-456-7890";

        // Act
        var error = new InvalidPhoneNumber(phoneNumber);

        // Assert
        Assert.AreEqual(phoneNumber, error.PhoneNumber);
    }

    [TestMethod]
    public void Constructor_ValidPhoneNumber_SetsMessage()
    {
        // Arrange
        const string phoneNumber = "123-456-7890";

        // Act
        var error = new InvalidPhoneNumber(phoneNumber);

        // Assert
        Assert.AreEqual("'123-456-7890' is not a valid phone number.", error.Message);
    }

    [TestMethod]
    public void Constructor_ValidPhoneNumber_SetsErrorCodeMetadata()
    {
        // Arrange
        const string phoneNumber = "123-456-7890";

        // Act
        var error = new InvalidPhoneNumber(phoneNumber);

        // Assert
        Assert.IsTrue(error.Metadata.ContainsKey("ErrorCode"));
        Assert.AreEqual("DOM_INVALID_PHONE_NUMBER", error.Metadata["ErrorCode"]);
    }

    [TestMethod]
    public void Constructor_ValidPhoneNumber_SetsTimeStampMetadata()
    {
        // Arrange
        const string phoneNumber = "123-456-7890";
        var before = DateTime.UtcNow;

        // Act
        var error = new InvalidPhoneNumber(phoneNumber);
        var after = DateTime.UtcNow;

        // Assert
        Assert.IsTrue(error.Metadata.ContainsKey("TimeStamp"));
        var timestamp = (DateTime)error.Metadata["TimeStamp"];
        Assert.IsTrue(timestamp >= before && timestamp <= after);
    }

    [TestMethod]
    public void Constructor_EmptyPhoneNumber_SetsPhoneNumberAndMessage()
    {
        // Arrange
        const string phoneNumber = "";

        // Act
        var error = new InvalidPhoneNumber(phoneNumber);

        // Assert
        Assert.AreEqual(string.Empty, error.PhoneNumber);
        Assert.AreEqual("'' is not a valid phone number.", error.Message);
    }

    [TestMethod]
    public void Constructor_NullPhoneNumber_SetsPhoneNumberToNullAndMessageWithoutValue()
    {
        // Arrange
        string? phoneNumber = null;

        // Act
        var error = new InvalidPhoneNumber(phoneNumber!);

        // Assert
        Assert.IsNull(error.PhoneNumber);
        Assert.AreEqual("'' is not a valid phone number.", error.Message);
    }
}
