# Scynett.Hubtel.Payments

A clean, modern, fully async .NET 9 SDK for integrating Hubtel Mobile Money payments (Receive Money, Status Check, Callbacks) with built-in CQRS, vertical slices, and extensible storage.

## Features

- ✅ **CQRS + Vertical Slices Architecture**: Clean separation of concerns with commands, queries, and handlers
- ✅ **ReceiveMoney Operations**: Init, Callback processing, and Status checking
- ✅ **Result<T> Pattern**: Type-safe error handling without exceptions
- ✅ **ASP.NET Core Integration**: DI extensions, callback endpoints, and background workers
- ✅ **Pending Transactions Worker**: Automatic status polling for pending transactions
- ✅ **.NET 9 Support**: Built with the latest .NET framework

## Projects

### Scynett.Hubtel.Payments

Core SDK library containing:
- `HubtelOptions`: Configuration for Hubtel API credentials
- `Result<T>` and `Error`: Type-safe result pattern
- `IReceiveMoneyService`: Service for initiating and processing mobile money payments
- `IHubtelStatusService`: Service for checking transaction status
- `IPendingTransactionsStore`: Interface for storing pending transactions (with in-memory implementation)

### Scynett.Hubtel.Payments.AspNetCore

ASP.NET Core integration library containing:
- DI extension methods (`AddHubtelPayments`)
- Callback endpoint mappings (`MapHubtelCallbacks`)
- Background worker for checking pending transactions

## Installation

```bash
dotnet add package Scynett.Hubtel.Payments
dotnet add package Scynett.Hubtel.Payments.AspNetCore
```

## Configuration

Add Hubtel settings to your `appsettings.json`:

```json
{
  "Hubtel": {
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "MerchantAccountNumber": "your-merchant-account",
    "BaseUrl": "https://api.hubtel.com",
    "TimeoutSeconds": 30
  }
}
```

## Usage

### 1. Register Services

In your `Program.cs`:

```csharp
using Scynett.Hubtel.Payments.AspNetCore.Extensions;
using Scynett.Hubtel.Payments.AspNetCore.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add Hubtel Payments SDK
builder.Services.AddHubtelPayments(builder.Configuration);

var app = builder.Build();

// Map Hubtel callback endpoint
app.MapHubtelCallbacks("/api/hubtel/callback");

app.Run();
```

### 2. Initialize Payment

```csharp
using Scynett.Hubtel.Payments.Features.ReceiveMoney;

public class PaymentController : ControllerBase
{
    private readonly IReceiveMoneyService _receiveMoneyService;

    public PaymentController(IReceiveMoneyService receiveMoneyService)
    {
        _receiveMoneyService = receiveMoneyService;
    }

    [HttpPost("payments/init")]
    public async Task<IActionResult> InitPayment([FromBody] PaymentRequest request)
    {
        var command = new InitReceiveMoneyCommand(
            CustomerName: request.CustomerName,
            CustomerMobileNumber: request.PhoneNumber,
            Channel: "mtn-gh", // or "vodafone-gh", "tigo-gh"
            Amount: request.Amount,
            Description: request.Description,
            ClientReference: Guid.NewGuid().ToString(),
            PrimaryCallbackUrl: "https://yourapp.com/api/hubtel/callback"
        );

        var result = await _receiveMoneyService.InitAsync(command);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }
}
```

### 3. Check Transaction Status

```csharp
using Scynett.Hubtel.Payments.Features.Status;

[HttpGet("payments/{transactionId}/status")]
public async Task<IActionResult> CheckStatus(
    string transactionId,
    [FromServices] IHubtelStatusService statusService)
{
    var query = new CheckStatusQuery(transactionId);
    var result = await statusService.CheckStatusAsync(query);

    if (result.IsFailure)
    {
        return NotFound(result.Error);
    }

    return Ok(result.Value);
}
```

## Architecture

### CQRS + Vertical Slices

Each feature is organized as a vertical slice with:
- **Commands**: `InitReceiveMoneyCommand`, `ReceiveMoneyCallbackCommand`
- **Queries**: `CheckStatusQuery`
- **Responses**: `InitReceiveMoneyResponse`, `CheckStatusResponse`
- **Services**: `ReceiveMoneyService`, `HubtelStatusService`

### Background Worker

The `PendingTransactionsWorker` automatically:
1. Polls pending transactions every 5 minutes
2. Checks their status using the Hubtel API
3. Processes completed transactions (success/failed)
4. Removes them from the pending store

## Extensibility

### Custom Pending Transactions Store

Implement `IPendingTransactionsStore` for persistent storage:

```csharp
public class RedisPendingTransactionsStore : IPendingTransactionsStore
{
    // Implementation using Redis
}

// Register in DI
services.AddSingleton<IPendingTransactionsStore, RedisPendingTransactionsStore>();
```

## License

MIT

