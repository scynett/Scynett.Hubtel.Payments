# Hubtel Event IDs - Organization & Reference

## Event ID Ranges

All event IDs are centralized in `Application/Common/HubtelEventIds.cs` for consistency and maintainability.

### Range Allocation

| Range | Category | Description |
|-------|----------|-------------|
| 100-109 | Payment Events | Generic payment-related events |
| 110-129 | DirectReceiveMoney - Initiate | Events specific to initiating receive money transactions |
| 130-149 | DirectReceiveMoney - Callback | Events specific to processing callbacks |
| 150-169 | DirectReceiveMoney - StatusCheck | (Reserved for future use) |
| 170-199 | DirectReceiveMoney - Other | (Reserved for future use) |
| 200-299 | Generic Callback Events | Legacy/generic callback events |
| 300-399 | Status Check Events | Transaction status checking |
| 400-499 | Worker Events | Background worker events |
| 500-599 | (Reserved) | Future expansion |

## Event ID Reference

### Generic Payment Events (100-109)

| Event ID | Constant | Level | Description |
|----------|----------|-------|-------------|
| 100 | `PaymentInitiating` | Information | Payment initialization started |
| 101 | `PaymentInitResponse` | Information | Payment initialization response received |
| 102 | `PaymentInitError` | Error | Payment initialization error |
| 103 | `TransactionPending` | Information | Transaction is pending |
| 104 | `TransactionCompleted` | Information | Transaction completed |

### DirectReceiveMoney - Initiate (110-129)

| Event ID | Constant | Level | Description |
|----------|----------|-------|-------------|
| 110 | `DirectReceiveMoneyInitiating` | Information | Initiating a receive money transaction |
| 111 | `DirectReceiveMoneyValidationFailed` | Warning | Request validation failed |
| 112 | `DirectReceiveMoneyDecisionComputed` | Information | Decision computed from response |
| 113 | `DirectReceiveMoneyPendingStored` | Information | Transaction stored as pending |
| 114 | `DirectReceiveMoneyPendingButMissingTransactionId` | Warning | Pending decision without transaction ID |
| 115 | `DirectReceiveMoneyGatewayFailed` | Error | Gateway call failed |
| 116 | `DirectReceiveMoneyUnhandledException` | Error | Unhandled exception during initiation |

### DirectReceiveMoney - Callback (130-149)

| Event ID | Constant | Level | Description |
|----------|----------|-------|-------------|
| 130 | `DirectReceiveMoneyCallbackReceived` | Information | Callback received from Hubtel |
| 131 | `DirectReceiveMoneyCallbackDecision` | Information | Decision computed from callback |
| 132 | `DirectReceiveMoneyCallbackPendingRemoved` | Information | Pending transaction removed |
| 133 | `DirectReceiveMoneyCallbackValidationFailed` | Warning | Callback validation failed |
| 134 | `DirectReceiveMoneyCallbackProcessingFailed` | Error | Callback processing failed |

### Generic Callback Events (200-299)

| Event ID | Constant | Level | Description |
|----------|----------|-------|-------------|
| 200 | `CallbackReceived` | Information | Generic callback received |
| 201 | `CallbackProcessing` | Information | Generic callback processing |
| 202 | `CallbackProcessed` | Information | Generic callback processed |
| 203 | `CallbackError` | Error | Generic callback error |
| 204 | `CallbackInvalidData` | Warning | Generic callback invalid data |

### Status Check Events (300-399)

| Event ID | Constant | Level | Description |
|----------|----------|-------|-------------|
| 300 | `StatusCheckStarted` | Information | Status check started |
| 301 | `StatusCheckCompleted` | Information | Status check completed |
| 302 | `StatusCheckFailed` | Warning | Status check failed |
| 303 | `StatusCheckError` | Error | Status check error |

### Worker Events (400-499)

| Event ID | Constant | Level | Description |
|----------|----------|-------|-------------|
| 400 | `WorkerStarted` | Information | Background worker started |
| 401 | `WorkerStopped` | Information | Background worker stopped |
| 402 | `WorkerCheckingTransactions` | Information | Worker checking transactions |
| 403 | `WorkerNoPendingTransactions` | Information | No pending transactions to check |
| 404 | `WorkerTransactionCheckFailed` | Warning | Transaction check failed |
| 405 | `WorkerError` | Error | Worker error |
| 406 | `WorkerTransactionError` | Error | Transaction-specific error |

## Usage Examples

### In LoggerMessage Attributes

```csharp
using Microsoft.Extensions.Logging;
using Scynett.Hubtel.Payments.Application.Common;

internal static partial class MyLogMessages
{
    [LoggerMessage(
        EventId = HubtelEventIds.DirectReceiveMoneyInitiating,
        Level = LogLevel.Information,
        Message = "Initiating transaction. ClientReference={ClientReference}")]
    public static partial void Initiating(
        ILogger logger,
        string clientReference);
}
```

### Querying Logs by Event ID

```csharp
// Application Insights / Azure Monitor query
// Find all initiation events
traces
| where customDimensions.EventId == 110
| project timestamp, message, customDimensions

// Find all callback-related events
traces
| where customDimensions.EventId between (130 .. 149)
| project timestamp, message, customDimensions

// Find all errors
traces
| where severityLevel >= 3  // Error level
| where customDimensions.EventId in (102, 115, 116, 134, 203, 303, 405, 406)
| project timestamp, message, customDimensions
```

### Structured Logging Benefits

With centralized event IDs:

1. **Filtering**: Easily filter logs by feature
   ```
   EventId >= 110 AND EventId <= 149  // All DirectReceiveMoney events
   ```

2. **Alerting**: Create alerts on specific event IDs
   ```
   Alert when EventId == 134 (CallbackProcessingFailed)
   ```

3. **Metrics**: Track event frequency
   ```
   Count EventId 130 (CallbackReceived) vs 134 (CallbackProcessingFailed)
   Success Rate = (130 - 134) / 130
   ```

4. **Dashboards**: Group logs by ID ranges
   ```
   Initiate Operations: 110-129
   Callback Operations: 130-149
   ```

## Best Practices

### When Adding New Event IDs

1. **Check Range**: Ensure the ID fits in the appropriate range
2. **Update Documentation**: Add to this document
3. **Use Constants**: Always use `HubtelEventIds.ConstantName`, never hardcode
4. **Descriptive Names**: Use clear, feature-specific constant names
5. **Sequential**: Use the next available ID in the range

### Naming Convention

```
[Feature][Component][Action]

Examples:
- DirectReceiveMoneyInitiating
- DirectReceiveMoneyCallbackReceived
- DirectReceiveMoneyCallbackProcessingFailed
```

### Level Guidelines

- **Information (100-299)**: Normal operations, state changes
- **Warning (300-399)**: Recoverable issues, validation failures
- **Error (400-499)**: Unrecoverable errors, exceptions
- **Critical (500+)**: System-wide failures (reserved)

## Migration Notes

### Before (Inconsistent)

```csharp
// Hardcoded IDs scattered across files
[LoggerMessage(EventId = 41001, ...)]
[LoggerMessage(EventId = 200, ...)]
[LoggerMessage(EventId = 1234, ...)]  // No pattern
```

### After (Centralized)

```csharp
// All IDs in HubtelEventIds.cs
[LoggerMessage(EventId = HubtelEventIds.DirectReceiveMoneyValidationFailed, ...)]
[LoggerMessage(EventId = HubtelEventIds.DirectReceiveMoneyCallbackReceived, ...)]
```

## Future Expansion

### Reserved Ranges

| Range | Reserved For |
|-------|--------------|
| 150-169 | DirectReceiveMoney - StatusCheck |
| 170-199 | DirectReceiveMoney - Other features |
| 500-599 | SendMoney feature |
| 600-699 | Refunds feature |
| 700-799 | Wallet feature |
| 800-899 | Webhooks/Events |
| 900-999 | Infrastructure/Cross-cutting |

## Summary

? **All event IDs now centralized in `HubtelEventIds.cs`**
? **Consistent naming convention applied**
? **Logical range allocation for easy filtering**
? **No more hardcoded event IDs**
? **Ready for Application Insights/telemetry**

---

**Last Updated**: 2024
**Maintained By**: SDK Team
