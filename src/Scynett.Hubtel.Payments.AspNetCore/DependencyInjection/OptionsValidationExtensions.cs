using Microsoft.Extensions.DependencyInjection;

using Scynett.Hubtel.Payments.Options;

namespace Scynett.Hubtel.Payments.AspNetCore.DependencyInjection;

/// <summary>
/// Startup validation of <see cref="HubtelOptions"/>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Behaviour change (SDK hardening):</b> this method used to be <c>internal</c> and was called by nothing,
/// so a missing <see cref="HubtelOptions.ClientId"/> / <see cref="HubtelOptions.ClientSecret"/> was only
/// discovered when a customer tried to pay and Hubtel answered <c>401</c>. It is now public and is also
/// invoked by <c>AddHubtelPayments(...)</c>, so a misconfigured app fails at startup instead of at the till.
/// </para>
/// </remarks>
public static class OptionsValidationExtensions
{
    /// <summary>
    /// Registers eager validation of <see cref="HubtelOptions"/>: the host will refuse to start if the
    /// Hubtel ClientId or ClientSecret is missing.
    /// </summary>
    /// <remarks>
    /// Safe to call more than once; <c>AddHubtelPayments(...)</c> already calls the equivalent registration.
    /// </remarks>
    public static IServiceCollection AddHubtelOptionsValidation(
        this IServiceCollection services)
    {
        services.AddOptions<HubtelOptions>()
            .Validate(o =>
                !string.IsNullOrWhiteSpace(o.ClientId) &&
                !string.IsNullOrWhiteSpace(o.ClientSecret),
                "Hubtel ClientId and ClientSecret must be provided")
            .ValidateOnStart();

        return services;
    }
}
