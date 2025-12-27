using FluentValidation;
using FluentValidation.Results;

using Scynett.Hubtel.Payments.Common;

namespace Scynett.Hubtel.Payments.Validation;

/// <summary>
/// Extension methods for FluentValidation integration with Result pattern.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Validates the request and returns a Result.
    /// </summary>
    public static Result ValidateToResult<T>(this IValidator<T> validator, T instance)
    {
        var validationResult = validator.Validate(instance);
        
        if (validationResult.IsValid)
        {
            return Result.Success();
        }

        var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
        return Result.Failure(new Error("Validation.Failed", errors));
    }

    /// <summary>
    /// Validates the request and returns a Result with typed value.
    /// </summary>
    public static Result<T> ValidateToResult<T>(this IValidator<T> validator, T instance, Func<T, Result<T>> onValid)
    {
        var validationResult = validator.Validate(instance);
        
        if (validationResult.IsValid)
        {
            return onValid(instance);
        }

        var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
        return Result.Failure<T>(new Error("Validation.Failed", errors));
    }

    /// <summary>
    /// Converts ValidationResult to Error.
    /// </summary>
    public static Error ToError(this ValidationResult validationResult)
    {
        var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
        return new Error("Validation.Failed", errors);
    }

    /// <summary>
    /// Gets all validation errors as a dictionary.
    /// </summary>
    public static Dictionary<string, string[]> GetErrorsDictionary(this ValidationResult validationResult)
    {
        return validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());
    }
}
