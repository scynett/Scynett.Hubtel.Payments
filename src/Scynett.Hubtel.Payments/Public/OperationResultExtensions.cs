using Scynett.Hubtel.Payments.Application.Common;

namespace Scynett.Hubtel.Payments;

public static class OperationResultExtensions
{
    public static T OrThrow<T>(this OperationResult<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.IsSuccess ? result.Value : throw new HubtelPaymentsException(result.Error);
    }
}
