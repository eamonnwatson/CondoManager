using CondoScope.Application.Common.Errors;
using CondoScope.Persistence.Repositories;
using FluentResults;
using Microsoft.Data.Sqlite;

namespace CondoScope.Persistence.UnitTests.Repositories;

[TestClass]
public class BaseRepositoryTests
{
    private sealed class TestRepository : BaseRepository
    {
        public static Task<Result<T>> CallExecuteAsync<T>(Func<Task<T>> operation) => ExecuteAsync(operation);

        public static Task<Result<T>> CallGetDataAsync<T>(Func<Task<T?>> operation, string notFoundMessage) =>
            GetDataAsync(operation, notFoundMessage);

        public static Task<Result> CallExecuteAsync(Func<Task> operation) => ExecuteAsync(operation);
    }

    [TestMethod]
    public async Task ExecuteAsyncOfT_OperationSucceeds_ReturnsOkResultWithValue()
    {
        // Arrange
        var expected = 42;

        // Act
        var result = await TestRepository.CallExecuteAsync(() => Task.FromResult(expected));

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(expected, result.Value);
    }

    [TestMethod]
    public async Task ExecuteAsyncOfT_OperationThrowsRegularException_ReturnsFailedResultWithUnexpectedAppError()
    {
        // Arrange
        var exception = new InvalidOperationException("boom");

        // Act
        var result = await TestRepository.CallExecuteAsync<int>(() => throw exception);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<UnexpectedAppError>(result.Errors[0]);
    }

    [TestMethod]
    public async Task ExecuteAsyncOfT_OperationThrowsSqliteException_ReturnsFailedResultWithDatabaseError()
    {
        // Arrange
        var exception = new SqliteException("sqlite failure", 1);

        // Act
        var result = await TestRepository.CallExecuteAsync<int>(() => throw exception);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<DatabaseError>(result.Errors[0]);
    }

    [TestMethod]
    public async Task ExecuteAsyncOfT_OperationThrowsWrappedSqliteException_ReturnsFailedResultWithDatabaseError()
    {
        // Arrange
        var innerException = new SqliteException("sqlite failure", 1);
        var wrapper = new InvalidOperationException("outer", innerException);

        // Act
        var result = await TestRepository.CallExecuteAsync<int>(() => throw wrapper);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<DatabaseError>(result.Errors[0]);
    }

    [TestMethod]
    public async Task GetDataAsync_OperationReturnsValue_ReturnsOkResultWithValue()
    {
        // Arrange
        var expected = "some-value";

        // Act
        var result = await TestRepository.CallGetDataAsync(() => Task.FromResult<string?>(expected), "not found");

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(expected, result.Value);
    }

    [TestMethod]
    public async Task GetDataAsync_OperationReturnsNull_ReturnsFailedResultWithNotFoundError()
    {
        // Arrange
        var notFoundMessage = "entity not found";

        // Act
        var result = await TestRepository.CallGetDataAsync(() => Task.FromResult<string?>(null), notFoundMessage);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<NotFoundError>(result.Errors[0]);
        Assert.AreEqual(notFoundMessage, result.Errors[0].Message);
    }

    [TestMethod]
    public async Task GetDataAsync_OperationThrowsRegularException_ReturnsFailedResultWithUnexpectedAppError()
    {
        // Arrange
        var exception = new InvalidOperationException("boom");

        // Act
        var result = await TestRepository.CallGetDataAsync<string>(() => throw exception, "not found");

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<UnexpectedAppError>(result.Errors[0]);
    }

    [TestMethod]
    public async Task GetDataAsync_OperationThrowsSqliteException_ReturnsFailedResultWithDatabaseError()
    {
        // Arrange
        var exception = new SqliteException("sqlite failure", 1);

        // Act
        var result = await TestRepository.CallGetDataAsync<string>(() => throw exception, "not found");

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<DatabaseError>(result.Errors[0]);
    }

    [TestMethod]
    public async Task ExecuteAsync_OperationSucceeds_ReturnsOkResult()
    {
        // Arrange
        var executed = false;

        // Act
        var result = await TestRepository.CallExecuteAsync(() =>
        {
            executed = true;
            return Task.CompletedTask;
        });

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(executed);
    }

    [TestMethod]
    public async Task ExecuteAsync_OperationThrowsRegularException_ReturnsFailedResultWithUnexpectedAppError()
    {
        // Arrange
        var exception = new InvalidOperationException("boom");

        // Act
        var result = await TestRepository.CallExecuteAsync(() => throw exception);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<UnexpectedAppError>(result.Errors[0]);
    }

    [TestMethod]
    public async Task ExecuteAsync_OperationThrowsSqliteException_ReturnsFailedResultWithDatabaseError()
    {
        // Arrange
        var exception = new SqliteException("sqlite failure", 1);

        // Act
        var result = await TestRepository.CallExecuteAsync(() => throw exception);

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.IsInstanceOfType<DatabaseError>(result.Errors[0]);
    }
}
