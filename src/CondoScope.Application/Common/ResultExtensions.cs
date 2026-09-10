using FluentResults;

namespace CondoScope.Application.Common;

public static class ResultExtensions
{
    // Map: Transforms a successful value into a new type
    public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> mapper)
    {
        if (result.IsFailed)
            return Result.Fail<TOut>(result.Errors);

        return Result.Ok(mapper(result.Value));
    }

    // Bind: Chains another operation that returns a Result
    public static Result<TOut> Bind<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> binder)
    {
        if (result.IsFailed)
            return Result.Fail<TOut>(result.Errors);

        return binder(result.Value);
    }

    // Bind Async: Input is an Async Task, Output function is Async
    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, Task<Result<TOut>>> binder)
    {
        var result = await resultTask;
        if (result.IsFailed)
            return Result.Fail<TOut>(result.Errors);

        return await binder(result.Value);
    }

    // Bind Async Overload: Input is a Synchronous Result, Output function is Async
    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<Result<TOut>>> binder)
    {
        if (result.IsFailed)
            return Result.Fail<TOut>(result.Errors);

        return await binder(result.Value);
    }

    // Map Async: Input is an Async Task, Output function is Synchronous
    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, TOut> mapper)
    {
        var result = await resultTask;
        if (result.IsFailed)
            return Result.Fail<TOut>(result.Errors);

        return Result.Ok(mapper(result.Value));
    }
}
