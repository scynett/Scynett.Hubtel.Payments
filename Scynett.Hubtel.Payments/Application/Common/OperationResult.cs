using CSharpFunctionalExtensions;

namespace Scynett.Hubtel.Payments.Application.Common;
#pragma warning disable CA1000 // Do not declare static members on generic types
public sealed class OperationResult<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    private readonly T? _value;
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("No value for a failed result.");

    public Error Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("No error for a successful result.");

    private readonly Error? _error;

    private OperationResult(bool isSuccess, T? value, Error? error)
    {
        // Invariants
        if (isSuccess && error is not null)
            throw new ArgumentException("Successful result cannot contain an error.", nameof(error));

        if (!isSuccess && error is null)
            throw new ArgumentException("Failed result must contain an error.", nameof(error));

        IsSuccess = isSuccess;
        _value = value;
        _error = error;
    }

    public static OperationResult<T> Success(T value)
        => new(true, value, null);


    public static OperationResult<T> Failure(Error error)
#pragma warning restore CA1000 // Do not declare static members on generic types
        => new(false, default, error);

#pragma warning disable CA1000 // Do not declare static members on generic types
    public static OperationResult<T> From(Result<T, Error> result)
#pragma warning restore CA1000 // Do not declare static members on generic types
        => result.IsSuccess ? Success(result.Value) : Failure(result.Error);
}

public class OperationResult
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    private readonly Error? _error;
    public Error Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("No error for a successful result.");

    protected OperationResult(bool isSuccess, Error? error)
    {
        if (isSuccess && error is not null)
            throw new ArgumentException("Successful result cannot contain an error.", nameof(error));

        if (!isSuccess && error is null)
            throw new ArgumentException("Failed result must contain an error.", nameof(error));

        IsSuccess = isSuccess;
        _error = error;
    }

    public static OperationResult Success() => new(true, null);

    public static OperationResult Failure(Error error) => new(false, error);

    public static OperationResult From(Result result, Error error)
#pragma warning disable CA1062 // Validate arguments of public methods
        => result.IsSuccess ? Success() : Failure(error);
#pragma warning restore CA1062 // Validate arguments of public methods
}