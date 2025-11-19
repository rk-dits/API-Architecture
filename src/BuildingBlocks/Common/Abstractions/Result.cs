using System.Diagnostics.CodeAnalysis;

namespace BuildingBlocks.Common.Abstractions;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None) throw new InvalidOperationException("Successful result cannot have an error");
        if (!isSuccess && error == Error.None) throw new InvalidOperationException("Failure result must have an error");
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);
    public static Result Failure(Error error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(value, true, Error.None);
    public static Result<T> Failure<T>(Error error) => new(default!, false, error);
}

public sealed class Result<T> : Result
{
    private readonly T _value;
    public T Value => IsSuccess ? _value : throw new InvalidOperationException("No value for failure result");

    internal Result(T value, bool isSuccess, Error error) : base(isSuccess, error)
    {
        _value = value;
    }

    public void Deconstruct(out bool isSuccess, out T value, out Error error)
    {
        isSuccess = IsSuccess;
        value = _value;
        error = Error;
    }

    public static implicit operator Result<T>(T value) => Result.Success(value);
}
