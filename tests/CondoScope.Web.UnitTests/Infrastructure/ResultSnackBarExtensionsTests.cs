using CondoScope.Web.Infrastructure;
using FluentResults;
using Moq;
using MudBlazor;

namespace CondoScope.Web.UnitTests.Infrastructure;

[TestClass]
public class ResultSnackBarExtensionsTests
{
    private readonly Mock<ISnackbar> snackbarMock = new(MockBehavior.Strict);

    [TestMethod]
    public void ShowErrorsIfFailed_ResultSuccess_ReturnsFalseAndDoesNotAddSnackbar()
    {
        // Arrange
        var result = Result.Ok();

        // Act
        var actual = result.ShowErrorsIfFailed(this.snackbarMock.Object);

        // Assert
        Assert.IsFalse(actual);
        this.snackbarMock.Verify(s => s.Add(It.IsAny<string>(), It.IsAny<Severity>(), null, null), Times.Never);
    }

    [TestMethod]
    public void ShowErrorsIfFailed_ResultFailedWithSingleError_ReturnsTrueAndAddsSnackbarMessage()
    {
        // Arrange
        var result = Result.Fail("Something went wrong");
        this.snackbarMock
            .Setup(s => s.Add("Something went wrong", Severity.Error, null, null))
            .Returns((Snackbar?)null!);

        // Act
        var actual = result.ShowErrorsIfFailed(this.snackbarMock.Object);

        // Assert
        Assert.IsTrue(actual);
        this.snackbarMock.Verify(s => s.Add("Something went wrong", Severity.Error, null, null), Times.Once);
    }

    [TestMethod]
    public void ShowErrorsIfFailed_ResultFailedWithMultipleErrors_ReturnsTrueAndAddsAllSnackbarMessages()
    {
        // Arrange
        var result = Result.Fail("First error").WithError("Second error");
        this.snackbarMock
            .Setup(s => s.Add("First error", Severity.Error, null, null))
            .Returns((Snackbar?)null!);
        this.snackbarMock
            .Setup(s => s.Add("Second error", Severity.Error, null, null))
            .Returns((Snackbar?)null!);

        // Act
        var actual = result.ShowErrorsIfFailed(this.snackbarMock.Object);

        // Assert
        Assert.IsTrue(actual);
        this.snackbarMock.Verify(s => s.Add("First error", Severity.Error, null, null), Times.Once);
        this.snackbarMock.Verify(s => s.Add("Second error", Severity.Error, null, null), Times.Once);
    }

    [TestMethod]
    public void ShowErrorsIfFailedGeneric_ResultSuccess_ReturnsFalseAndDoesNotAddSnackbar()
    {
        // Arrange
        var result = Result.Ok(42);

        // Act
        var actual = result.ShowErrorsIfFailed(this.snackbarMock.Object);

        // Assert
        Assert.IsFalse(actual);
        this.snackbarMock.Verify(s => s.Add(It.IsAny<string>(), It.IsAny<Severity>(), null, null), Times.Never);
    }

    [TestMethod]
    public void ShowErrorsIfFailedGeneric_ResultFailedWithSingleError_ReturnsTrueAndAddsSnackbarMessage()
    {
        // Arrange
        var result = Result.Fail<int>("Generic error");
        this.snackbarMock
            .Setup(s => s.Add("Generic error", Severity.Error, null, null))
            .Returns((Snackbar?)null!);

        // Act
        var actual = result.ShowErrorsIfFailed(this.snackbarMock.Object);

        // Assert
        Assert.IsTrue(actual);
        this.snackbarMock.Verify(s => s.Add("Generic error", Severity.Error, null, null), Times.Once);
    }

    [TestMethod]
    public void ShowErrorsIfFailedGeneric_ResultFailedWithMultipleErrors_ReturnsTrueAndAddsAllSnackbarMessages()
    {
        // Arrange
        var result = Result.Fail<int>("First generic error").WithError("Second generic error");
        this.snackbarMock
            .Setup(s => s.Add("First generic error", Severity.Error, null, null))
            .Returns((Snackbar?)null!);
        this.snackbarMock
            .Setup(s => s.Add("Second generic error", Severity.Error, null, null))
            .Returns((Snackbar?)null!);

        // Act
        var actual = result.ShowErrorsIfFailed(this.snackbarMock.Object);

        // Assert
        Assert.IsTrue(actual);
        this.snackbarMock.Verify(s => s.Add("First generic error", Severity.Error, null, null), Times.Once);
        this.snackbarMock.Verify(s => s.Add("Second generic error", Severity.Error, null, null), Times.Once);
    }
}
