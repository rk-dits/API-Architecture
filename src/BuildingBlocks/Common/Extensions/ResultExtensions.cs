using BuildingBlocks.Common.Abstractions;

namespace BuildingBlocks.Common.Extensions;

public static class ResultExtensions
{
    public static Result<TOut> Bind<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> func)
        => result.IsFailure ? Result.Failure<TOut>(result.Error) : func(result.Value);

    public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> func)
        => result.IsFailure ? Result.Failure<TOut>(result.Error) : Result.Success(func(result.Value));

    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<Result<TOut>>> func)
        => result.IsFailure ? Result.Failure<TOut>(result.Error) : await func(result.Value);

    public static async Task<Result<TOut>> MapAsync<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<TOut>> func)
        => result.IsFailure ? Result.Failure<TOut>(result.Error) : Result.Success(await func(result.Value));
}
