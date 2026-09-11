using CondoScope.Application.Common.Errors;
using FluentResults;
using Microsoft.Data.Sqlite;

namespace CondoScope.Persistence.Repositories;

internal abstract class BaseRepository
{
    /// <summary>
    /// Executes a database operation, returning the result wrapped in a <see cref="Result{T}"/>.
    /// SQLite failures are mapped to <see cref="DatabaseError"/>, any other exception is mapped
    /// to <see cref="UnexpectedAppError"/>.
    /// </summary>
    protected static async Task<Result<T>> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        try
        {
            var value = await operation().ConfigureAwait(false);
            return Result.Ok(value);
        }
        catch (Exception ex)
        {
            return Result.Fail<T>(ToApplicationError(ex));
        }
    }

    /// <summary>
    /// Executes a database operation with no return value, returning a <see cref="Result"/>.
    /// SQLite failures are mapped to <see cref="DatabaseError"/>, any other exception is mapped
    /// to <see cref="UnexpectedAppError"/>.
    /// </summary>
    protected static async Task<Result> ExecuteAsync(Func<Task> operation)
    {
        try
        {
            await operation().ConfigureAwait(false);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(ToApplicationError(ex));
        }
    }

    private static IError ToApplicationError(Exception ex)
    {
        return ContainsSqliteException(ex)
            ? new DatabaseError(ex)
            : new UnexpectedAppError(ex);
    }

    private static bool ContainsSqliteException(Exception? ex)
    {
        while (ex is not null)
        {
            if (ex is SqliteException)
            {
                return true;
            }

            ex = ex.InnerException;
        }

        return false;
    }
}
