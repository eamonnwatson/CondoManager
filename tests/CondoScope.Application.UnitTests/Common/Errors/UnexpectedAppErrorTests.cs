using CondoScope.Application.Common.Errors;

namespace CondoScope.Application.UnitTests.Common.Errors;

[TestClass]
public class UnexpectedAppErrorTests
{
    [TestMethod]
    public void Constructor_ValidException_SetsExpectedMessage()
    {
        // Arrange
        var exception = new InvalidOperationException("something broke");

        // Act
        var error = new UnexpectedAppError(exception);

        // Assert
        Assert.AreEqual("An unexpected error occurred.", error.Message);
    }

    [TestMethod]
    public void Constructor_ValidException_SetsExceptionProperty()
    {
        // Arrange
        var exception = new InvalidOperationException("something broke");

        // Act
        var error = new UnexpectedAppError(exception);

        // Assert
        Assert.AreSame(exception, error.Exception);
    }

    [TestMethod]
    public void Constructor_ValidException_SetsErrorCodeMetadata()
    {
        // Arrange
        var exception = new InvalidOperationException("something broke");

        // Act
        var error = new UnexpectedAppError(exception);

        // Assert
        Assert.IsTrue(error.Metadata.ContainsKey("ErrorCode"));
        Assert.AreEqual("APP_UNEXPECTED_ERROR", error.Metadata["ErrorCode"]);
    }

    [TestMethod]
    public void Constructor_ValidException_SetsTimeStampMetadata()
    {
        // Arrange
        var exception = new InvalidOperationException("something broke");
        var before = DateTime.UtcNow;

        // Act
        var error = new UnexpectedAppError(exception);
        var after = DateTime.UtcNow;

        // Assert
        Assert.IsTrue(error.Metadata.ContainsKey("TimeStamp"));
        var timeStamp = (DateTime)error.Metadata["TimeStamp"];
        Assert.IsTrue(timeStamp >= before && timeStamp <= after);
    }
}
