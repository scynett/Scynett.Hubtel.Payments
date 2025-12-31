# Scynett.Hubtel.Payments

A lightweight .NET SDK that wraps Hubtel's Direct Receive Money endpoints (initiate, callbacks, and status checks) with opinionated validation, hosted background workers, and DI-friendly abstractions.

> Targets **.NET 9/10** and is published as the `Scynett.Hubtel.Payments` and `Scynett.Hubtel.Payments.AspNetCore` NuGet packages.

## Highlights

- **Unified facade** - `IDirectReceiveMoney` exposes `InitiateAsync`, `HandleCallbackAsync`, and `CheckStatusAsync`, each returning the SDK's `OperationResult<T>` for predictable success/failure envelopes.
- **Refit-powered gateways** - strongly typed clients for Hubtel's Receive Money + Transaction Status APIs with Basic-auth handler, configurable base addresses, and pluggable resilience.
- **Hosted background worker** - `PendingTransactionsWorker` polls stored transactions after a grace period so you always get a final state even when callbacks are missed.
- **Ready-to-use storage** - ships with an in-memory `IPendingTransactionsStore` and extension points for Redis/SQL/custom stores.
- **ASP.NET Core friendly** - a single `AddHubtelPayments(...)` registration wires validators, processors, hosted worker, and Refit clients; the optional ASP.NET Core package re-exports the same registration.

## Packages

```bash
dotnet add package Scynett.Hubtel.Payments
dotnet add package Scynett.Hubtel.Payments.AspNetCore   # optional convenience wrapper
```

## Quickstart

### 1. Configure options & register services

```csharp
using Scynett.Hubtel.Payments.Options;
using Scynett.Hubtel.Payments.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOptions<HubtelOptions>()
    .Bind(builder.Configuration.GetSection(HubtelOptions.SectionName));

builder.Services.AddOptions<DirectReceiveMoneyOptions>().Configure(o =>
{
    o.PosSalesId = "POS-123"; // Hubtel POS / Merchant ID
});

builder.Services.AddHubtelPayments(worker =>
{
    worker.CallbackGracePeriod = TimeSpan.FromMinutes(5);
    worker.PollInterval = TimeSpan.FromSeconds(30);
    worker.BatchSize = 200;
});

var app = builder.Build();
app.Run();
```

The ASP.NET Core package exposes `services.AddHubtelPaymentsAspNetCore()` if you prefer a single call that forwards to the same registration.

### 2. Initiate a payment

```csharp
using Microsoft.AspNetCore.Mvc;
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Initiate;
using Scynett.Hubtel.Payments.DirectReceiveMoney;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly IDirectReceiveMoney _direct;

    public PaymentsController(IDirectReceiveMoney direct) => _direct = direct;

    [HttpPost("initiate")]
    public async Task<IActionResult> Initiate([FromBody] InitiateReceiveMoneyRequest request, CancellationToken ct)
    {
        var result = await _direct.InitiateAsync(request, ct);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
```

`InitiateReceiveMoneyRequest` matches Hubtel's payload (customer details, channel `mtn-gh | vodafone-gh | tigo-gh`, amount, callback URL, and client reference). Validation rules are enforced automatically.

### 3. Handle callbacks

```csharp
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Callback;

app.MapPost("/hubtel/callback", async (
    [FromBody] ReceiveMoneyCallbackRequest payload,
    IDirectReceiveMoney direct,
    CancellationToken ct) =>
{
    var result = await direct.HandleCallbackAsync(payload, ct);
    return result.IsSuccess ? Results.Ok() : Results.BadRequest(result.Error);
});
```

When the callback's response code is final, the worker removes the transaction from whatever `IPendingTransactionsStore` is registered.

### 4. (Optional) Manually query status

```csharp
using Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Status;

app.MapGet("/payments/{clientReference}/status", async (
    string clientReference,
    IDirectReceiveMoney direct,
    CancellationToken ct) =>
{
    var result = await direct.CheckStatusAsync(
        new TransactionStatusQuery(ClientReference: clientReference),
        ct);

    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
});
```

The hosted worker already performs periodic checks for pending items, but you can expose manual status lookups whenever you need them.

## Configuration reference

| Option | Description |
|--------|-------------|
| `HubtelOptions.ClientId` / `ClientSecret` | API credentials used by the Basic-auth delegating handler. |
| `HubtelOptions.MerchantAccountNumber` | Merchant POS/Sales ID (fallback when `DirectReceiveMoneyOptions.PosSalesId` is empty). |
| `HubtelOptions.ReceiveMoneyBaseAddress` / `TransactionStatusBaseAddress` | Override Hubtel endpoints (defaults to Hubtel production URLs). |
| `HubtelOptions.TimeoutSeconds` | Applied to both Refit `HttpClient` instances. |
| `DirectReceiveMoneyOptions.PosSalesId` | Explicit POS Sales ID for initiate/status calls. |
| `PendingTransactionsWorkerOptions.CallbackGracePeriod` | Minimum wait before contacting Hubtel after initiation. |
| `PendingTransactionsWorkerOptions.PollInterval` | Delay between worker batches (also used by `ExecuteAsync`). |
| `PendingTransactionsWorkerOptions.BatchSize` | Number of pending transactions processed per batch. |

All options are `IOptions<T>` friendly and can be bound from configuration or populated programmatically.

## Pending transactions store

The SDK registers `IPendingTransactionsStore` via `InMemoryPendingTransactionsStore` by default. Swap it for a persistent implementation by registering your type *before* calling `AddHubtelPayments`:

```csharp
services.AddSingleton<IPendingTransactionsStore, RedisPendingTransactionsStore>();
services.AddHubtelPayments();
```

The worker simply calls `GetAllAsync`, `AddAsync`, and `RemoveAsync`, so any durable medium (SQL, Redis, Cosmos DB, etc.) works.

## Public API surface

- `InitiateReceiveMoneyRequest` -> `OperationResult<InitiateReceiveMoneyResult>` (Hubtel response + decision metadata).
- `ReceiveMoneyCallbackRequest` -> `OperationResult<ReceiveMoneyCallbackResult>` (ensures payload is valid, removes pending when final).
- `TransactionStatusQuery` -> `OperationResult<TransactionStatusResult>` (supports client reference, Hubtel transaction ID, or network transaction ID).

Every command/result uses the shared `OperationResult<T>` + `Error` types so you can pattern match on `.IsSuccess` without exceptions.

## Testing & samples

The repository includes extensive unit and WireMock-backed integration tests under `tests/Scynett.Hubtel.Payments.Tests`. Use them as a reference for custom stubs, DI bootstrapping, or integration pipelines.

## License

MIT





