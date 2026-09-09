using CondoScope.Domain.Errors;

namespace CondoScope.Domain.UnitTests.Errors;

[TestClass]
public class InvalidEmailErrorTests
{
    [TestMethod]
    public void Constructor_ValidEmail_SetsEmailProperty()
    {
        // Arrange
        const string email = "test@example.com";

        // Act
        var error = new InvalidEmailError(email);

        // Assert
        Assert.AreEqual(email, error.Email);
    }

    [TestMethod]
    public void Constructor_ValidEmail_SetsExpectedMessage()
    {
        // Arrange
        const string email = "test@example.com";

        // Act
        var error = new InvalidEmailError(email);

        // Assert
        Assert.AreEqual("'test@example.com' is not a valid email address.", error.Message);
    }

    [TestMethod]
    public void Constructor_ValidEmail_SetsErrorCodeMetadata()
    {
        // Arrange
        const string email = "test@example.com";

        // Act
        var error = new InvalidEmailError(email);

        // Assert
        Assert.IsTrue(error.Metadata.ContainsKey("ErrorCode"));
        Assert.AreEqual("DOM_INVALID_EMAIL", error.Metadata["ErrorCode"]);
    }

    [TestMethod]
    public void Constructor_ValidEmail_SetsTimeStampMetadata()
    {
        // Arrange
        const string email = "test@example.com";
        var before = DateTime.UtcNow;

        // Act
        var error = new InvalidEmailError(email);
        var after = DateTime.UtcNow;

        // Assert
        Assert.IsTrue(error.Metadata.ContainsKey("TimeStamp"));
        var timeStamp = (DateTime)error.Metadata["TimeStamp"];
        Assert.IsTrue(timeStamp >= before && timeStamp <= after);
    }

    [TestMethod]
    public void Constructor_EmptyEmail_SetsEmailAndMessage()
    {
        // Arrange
        const string email = "";

        // Act
        var error = new InvalidEmailError(email);

        // Assert
        Assert.AreEqual(email, error.Email);
        Assert.AreEqual("'' is not a valid email address.", error.Message);
    }

    [TestMethod]
    public void Constructor_NullEmail_SetsEmailAndMessageWithEmptyInterpolation()
    {
        // Arrange
        string? email = null;

        // Act
        var error = new InvalidEmailError(email!);

        // Assert
        Assert.IsNull(error.Email);
        Assert.AreEqual("'' is not a valid email address.", error.Message);
    }
}
