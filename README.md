# Scynett.Hubtel.Payments
A production-grade .NET SDK for Hubtel Direct Receive Money, callbacks, and transaction status checks with opinionated validation, idempotent processing, and DI-friendly registrations.

## Build status & packages
[![CI](https://github.com/scynett/Scynett.Hubtel.Payments/actions/workflows/ci.yml/badge.svg)](https://github.com/scynett/Scynett.Hubtel.Payments/actions/workflows/ci.yml)
[![Release](https://github.com/scynett/Scynett.Hubtel.Payments/actions/workflows/release.yml/badge.svg)](https://github.com/scynett/Scynett.Hubtel.Payments/actions/workflows/release.yml)
[![NuGet](https://img.shields.io/nuget/v/ScynettPayments.svg)](https://www.nuget.org/packages/ScynettPayments)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

---

## Quick Start

### Install
```bash
dotnet add package Scynett.Hubtel.Payments
dotnet add package Scynett.Hubtel.Payments.AspNetCore        # optional extensions
dotnet add package Scynett.Hubtel.Payments.Storage.PostgreSql # optional persistent store
```

### Configure services
```csharp
using Scynett.Hubtel.Payments.DependencyInjection;
using Scynett.Hubtel.Payments.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<HubtelOptions>()
    .Bind(builder.Configuration.GetSection(HubtelOptions.SectionName));

builder.Services.AddOptions<DirectReceiveMoneyOptions>().Configure(o =>
{
    o.PosSalesId = "POS-123"; // fallback POS Sales ID
});

builder.Services.AddHubtelPayments();
builder.Services.AddHubtelPaymentsWorker(); // opt-in polling worker

var app = builder.Build();
app.UseHubtelCorrelation(); // correlates inbound callbacks with outbound requests
app.MapGet("/", () => "OK");
app.Run();
```

### Initiate a payment
```csharp
[ApiController]
[Route("api/payments")]
public sealed class PaymentsController : ControllerBase
{
    private readonly IDirectReceiveMoney _direct;

    public PaymentsController(IDirectReceiveMoney direct) => _direct = direct;

    [HttpPost("initiate")]
    public async Task<IActionResult> Initiate(
        [FromBody] InitiateReceiveMoneyRequest request,
        CancellationToken ct)
    {
        var result = await _direct.InitiateAsync(request, ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
```

---

## Features
- ✅ Direct Receive Money initiation (`InitiateAsync`).
- ✅ Callback processing with validation, decision mapping, and audit store.
- ✅ Transaction status checks (`CheckStatusAsync`) with Refit clients.
- ✅ `OperationResult<T>` envelope + rich `Error` metadata (ProviderCode, ProviderMessage, metadata dictionary).
- ✅ Opt-in `PendingTransactionsWorker` and persistent store abstractions.
- ✅ Correlation + observability: ActivitySource instrumentation, `X-Correlation-Id` propagation, structured logging hooks.
- ✅ Handles Hubtel error codes and categorizes responses (success/pending/config errors/etc.).
- 🧭 Roadmap: additional Hubtel APIs (payouts, refunds), more storage providers, docs site.

---

## Requirements
- .NET 9.0 or later (SDK/Runtime). Tests also target .NET 10 for forward-compatibility.
- Hubtel API credentials (Client ID, Client Secret).
- A public HTTPS callback endpoint for Receive Money callbacks.
- If persistence is required, configure a durable `IPendingTransactionsStore` (PostgreSQL package provided.).

---

## Configuration

### Required options (`HubtelOptions`)
| Property | Description |
|----------|-------------|
| `ClientId` | Hubtel API client ID (Basic auth username). |
| `ClientSecret` | Hubtel API client secret (Basic auth password). |
| `MerchantAccountNumber` | POS Sales ID (used unless overridden in `DirectReceiveMoneyOptions`). |
| `ReceiveMoneyBaseAddress` / `TransactionStatusBaseAddress` | Base URLs for Hubtel endpoints (defaults to Hubtel production). |
| `TimeoutSeconds` | HttpClient timeout applied to Refit clients. |

### Optional options
| Options class | Key properties |
|---------------|----------------|
| `DirectReceiveMoneyOptions` | `PosSalesId` override, `DefaultCallbackAddress`. |
| `PendingTransactionsWorkerOptions` | `CallbackGracePeriod`, `PollInterval`, `BatchSize`. |
| `PendingTransactionsCleanupOptions` | `RetentionPeriod`, `CleanupInterval`. |
| `PostgreSqlStorageOptions` (if using SQL store) | `ConnectionString`, `SchemaName`, `TableName`, `AutoCreateSchema`. |

Example `appsettings.json`:
```json
{
  "Hubtel": {
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret",
    "MerchantAccountNumber": "POS-123",
    "ReceiveMoneyBaseAddress": "https://rmp.hubtel.com",
    "TransactionStatusBaseAddress": "https://api-txnstatus.hubtel.com"
  },
  "DirectReceiveMoney": {
    "PosSalesId": "POS-123",
    "DefaultCallbackAddress": "https://myapp.example.com/hubtel/callback"
  },
  "Hubtel:Storage:PostgreSql": {
    "ConnectionString": "Host=localhost;Database=hubtel;Username=user;Password=pass",
    "SchemaName": "hubtel",
    "TableName": "pending_transactions"
  }
}
```

---

## Error handling model
All entry points return `OperationResult<T>`:
```csharp
var result = await _direct.InitiateAsync(request, ct);
if (result.IsSuccess)
{
    // result.Value is InitiateReceiveMoneyResult
}
else
{
    // result.Error is Error (Code, Description, ProviderCode, ProviderMessage, Metadata)
}
```
Common error codes:
| Code | Meaning |
|------|---------|
| `DirectReceiveMoney.ValidationFailed` | FluentValidation rejected the request. |
| `DirectReceiveMoney.MissingPosSalesId` | No POS Sales ID configured. |
| `DirectReceiveMoney.UnhandledException` | Unexpected exception (see `error.Metadata["exception"]`). |
| `Hubtel.Callback.Validation` | Incoming callback payload invalid. |
| `Hubtel.Callback.Exception` | Exception while processing callback. |
| `TransactionStatus.InvalidQuery` | Status query missing identifiers. |
| `Hubtel.StatusCheckFailed` | Hubtel returned a non-success response code for status check. |

Inspect `Error.ProviderCode`/`ProviderMessage` to surface native Hubtel error codes to operators or to control retry logic.

---

## Versioning & releases
- Uses [Semantic Versioning](https://semver.org/): MAJOR.MINOR.PATCH.
- Releases are driven by git tags that start with `v` (e.g., `v1.2.3`, `v1.2.3-rc.1`).
- Annotated tag example:
  ```bash
  git tag -a v1.4.0 -m "Release v1.4.0"
  git push origin v1.4.0
  ```
- CI publishes packages to NuGet when the release workflow runs on a tag.
- See [CONTRIBUTING.md](CONTRIBUTING.md#releasing) for full release and verification steps.

---

## Security notes
- Always use HTTPS callback URLs and enforce domain/IP allowlists where possible.
- The `ICallbackValidator` abstraction allows shared-secret or IP-based validation of callbacks.
- Do not log `ClientSecret` or raw Hubtel payloads; the SDK already masks MSISDN except when explicitly needed.
- Rotate credentials regularly and scope Hubtel API keys to the minimum required permissions.

---

## Roadmap
- [ ] Support additional Hubtel APIs (payouts, refunds, account info).
- [ ] Additional storage providers (Redis, Azure Table Storage).
- [ ] Docs site with deeper guides and troubleshooting.
- [ ] More ASP.NET Core helpers (attribute routing, webhook signature filters).

---

## Contributing
Contributions are welcome! See [CONTRIBUTING.md](CONTRIBUTING.md) for development workflow, release process, and code style guidelines.

---

## License
[MIT](LICENSE)
