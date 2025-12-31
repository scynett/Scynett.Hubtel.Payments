using System.Collections.Generic;

namespace Scynett.Hubtel.Payments.Application.Common;

#pragma warning disable CA1716 // Identifiers should not match keywords
public record Error(string Code, string Description, ErrorType Type)
#pragma warning restore CA1716 // Identifiers should not match keywords
{
    public static readonly Error NullValue =
        Validation("General.Null", "Null value was provided");

    public string? ProviderCode { get; init; }
    public string? ProviderMessage { get; init; }
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }

    public static Error Failure(string code, string description) =>
        new(NormalizeCode(code, "General.Failure"), NormalizeDescription(description), ErrorType.Failure);

    public static Error NotFound(string code, string description) =>
        new(NormalizeCode(code, "General.NotFound"), NormalizeDescription(description), ErrorType.NotFound);

    public static Error Problem(string code, string description) =>
        new(NormalizeCode(code, "General.Problem"), NormalizeDescription(description), ErrorType.Problem);

    public static Error Conflict(string code, string description) =>
        new(NormalizeCode(code, "General.Conflict"), NormalizeDescription(description), ErrorType.Conflict);

    public static Error Validation(string code, string description) =>
        new(NormalizeCode(code, "General.Validation"), NormalizeDescription(description), ErrorType.Validation);

    public static Error From<TEnum>(TEnum code, string description, ErrorType type = ErrorType.Failure)
        where TEnum : Enum
        => new($"{typeof(TEnum).Name}.{code}", NormalizeDescription(description), type);

    public Error WithProvider(string? providerCode, string? providerMessage)
        => this with
        {
            ProviderCode = providerCode,
            ProviderMessage = providerMessage
        };

    public Error WithMetadata(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            return this;

        var updated = Metadata is null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(Metadata, StringComparer.OrdinalIgnoreCase);

        updated[key] = value;
        return this with { Metadata = updated };
    }

    public override string ToString() => $"{Code}: {Description}";

    private static string NormalizeCode(string code, string fallback)
        => string.IsNullOrWhiteSpace(code) ? fallback : code.Trim();

    private static string NormalizeDescription(string description)
        => string.IsNullOrWhiteSpace(description) ? "An error occurred." : description.Trim();
}
