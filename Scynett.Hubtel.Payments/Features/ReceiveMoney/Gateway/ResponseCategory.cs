namespace Scynett.Hubtel.Payments.Features.ReceiveMoney.Gateway;

public enum ResponseCategory
{
    Success,
    Pending,
    CustomerError,
    ValidationError,
    ConfigurationError,
    PermissionError,
    TransientError,
    Unknown
}
