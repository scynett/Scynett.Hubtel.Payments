namespace Scynett.Hubtel.Payments.Infrastructure.Http.Refit.DirectReceiveMoney.Dtos;


/// <summary>
/// Represents an error payload returned by Hubtel when the HTTP request fails
/// (e.g. authentication, authorization, or server errors).
/// </summary>
#pragma warning disable CA1812 // Avoid uninstantiated internal classes - Instantiated by Refit for error responses
internal sealed record HubtelApiErrorDto
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
{
    public int? Status { get; init; }
    public string? Error { get; init; }
    public string? Message { get; init; }
    public string? Path { get; init; }
    public string? Timestamp { get; init; }
}