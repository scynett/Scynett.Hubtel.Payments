using Microsoft.AspNetCore.Http;

namespace Scynett.Hubtel.Payments.AspNetCore.DirectReceiveMoney.Callback;

public interface ICallbackValidator
{
    Task<CallbackValidationResult> ValidateAsync(HttpContext context, CancellationToken cancellationToken = default);
}

public sealed record CallbackValidationResult(bool IsValid, string? ErrorCode, string? ErrorMessage)
{
    public static CallbackValidationResult Success { get; } = new(true, null, null);

    public static CallbackValidationResult Failure(string code, string message)
        => new(false, code, message);
}
