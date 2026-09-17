using CondoScope.Application.Common.Errors;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CondoScope.Application.UnitTests.Common.Errors;

[TestClass]
public class NotFoundErrorTests
{
    [TestMethod]
    public void Constructor_WithMessage_SetsMessage()
    {
        // Arrange
        const string message = "Resource not found";

        // Act
        var error = new NotFoundError(message);

        // Assert
        Assert.AreEqual(message, error.Message);
    }

    [TestMethod]
    public void Constructor_WithMessage_SetsErrorCodeMetadata()
    {
        // Arrange
        const string message = "Resource not found";

        // Act
        var error = new NotFoundError(message);

        // Assert
        Assert.IsTrue(error.Metadata.ContainsKey("ErrorCode"));
        Assert.AreEqual("APP_NOT_FOUND_ERROR", error.Metadata["ErrorCode"]);
    }

    [TestMethod]
    public void Constructor_WithMessage_SetsTimeStampMetadataAsDateTime()
    {
        // Arrange
        const string message = "Resource not found";
        var beforeCreation = DateTime.UtcNow;

        // Act
        var error = new NotFoundError(message);
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.IsTrue(error.Metadata.ContainsKey("TimeStamp"));
        var timeStamp = Assert.IsInstanceOfType<DateTime>(error.Metadata["TimeStamp"]);
        Assert.IsTrue(timeStamp >= beforeCreation && timeStamp <= afterCreation);
    }

    [TestMethod]
    public void Constructor_WithMessage_MetadataHasExactlyTwoEntries()
    {
        // Arrange
        const string message = "Resource not found";

        // Act
        var error = new NotFoundError(message);

        // Assert
        Assert.AreEqual(2, error.Metadata.Count);
    }

    [TestMethod]
    public void Constructor_WithEmptyMessage_SetsEmptyMessage()
    {
        // Arrange
        const string message = "";

        // Act
        var error = new NotFoundError(message);

        // Assert
        Assert.AreEqual(message, error.Message);
    }
}
