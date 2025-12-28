namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;

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
