using CondoScope.Application.Common.Errors;

namespace CondoScope.Application.UnitTests.Common.Errors;

[TestClass]
public class DatabaseErrorTests
{
    [TestMethod]
    public void Constructor_ValidException_SetsExpectedMessage()
    {
        // Arrange
        var exception = new InvalidOperationException("connection failed");

        // Act
        var error = new DatabaseError(exception);

        // Assert
        Assert.AreEqual("A database error occurred.", error.Message);
    }

    [TestMethod]
    public void Constructor_ValidException_SetsExceptionProperty()
    {
        // Arrange
        var exception = new InvalidOperationException("connection failed");

        // Act
        var error = new DatabaseError(exception);

        // Assert
        Assert.AreSame(exception, error.Exception);
    }

    [TestMethod]
    public void Constructor_ValidException_SetsErrorCodeMetadata()
    {
        // Arrange
        var exception = new InvalidOperationException("connection failed");

        // Act
        var error = new DatabaseError(exception);

        // Assert
        Assert.IsTrue(error.Metadata.ContainsKey("ErrorCode"));
        Assert.AreEqual("APP_DATABASE_ERROR", error.Metadata["ErrorCode"]);
    }

    [TestMethod]
    public void Constructor_ValidException_SetsTimeStampMetadata()
    {
        // Arrange
        var exception = new InvalidOperationException("connection failed");
        var before = DateTime.UtcNow;

        // Act
        var error = new DatabaseError(exception);
        var after = DateTime.UtcNow;

        // Assert
        Assert.IsTrue(error.Metadata.ContainsKey("TimeStamp"));
        var timeStamp = (DateTime)error.Metadata["TimeStamp"];
        Assert.IsTrue(timeStamp >= before && timeStamp <= after);
    }
}
