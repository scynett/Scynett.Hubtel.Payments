using Scynett.Hubtel.Payments.Application.Common;

namespace Scynett.Hubtel.Payments;

/// <summary>
/// Exception thrown when a Hubtel operation result represents a failure and the consumer opts into exception-based flows.
/// </summary>
public sealed class HubtelPaymentsException : Exception
{
    public HubtelPaymentsException()
        : this(Error.Problem("Hubtel.Exception", "Hubtel operation failed."))
    {
    }

    public HubtelPaymentsException(string message)
        : this(Error.Problem("Hubtel.Exception", message))
    {
    }

    public HubtelPaymentsException(string message, Exception innerException)
        : this(Error.Problem("Hubtel.Exception", message), innerException)
    {
    }

    public HubtelPaymentsException(Error error, Exception? innerException = null)
        : base(FormatMessage(error), innerException)
    {
        Error = error;
    }

    public Error Error { get; }

    private static string FormatMessage(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return $"{error.Code}: {error.Description}";
    }
}
