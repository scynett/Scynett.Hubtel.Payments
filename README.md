# Scynett.Hubtel.Payments

**Scynett.Hubtel.Payments** is a clean, modern, fully async **.NET SDK** for integrating with the **Hubtel Sales API** – starting with **Direct Receive Money** and **Transaction Status Check**.

It is designed with:

- ✅ **Clean public API** (easy to use, hard to misuse)
- ✅ **CQRS + Vertical Slice** internally
- ✅ **Background status checks** (5-minute Hubtel requirement)
- ✅ **Pluggable persistence** (you decide how/where to store transactions)
- ✅ **First-class ASP.NET Core support**

> ⚠️ **Status:** Early development (pre-release). API surface may still change.

---

## ✨ Features (v0)

- **Direct Receive Money**
  - Initiate MoMo payments (MTN, Vodafone, AirtelTigo)
  - Strongly-typed request/response models
- **Callbacks**
  - Handle Hubtel’s async callback payload in a single endpoint
- **Status Check**
  - Query final transaction status using `clientReference`
- **Result-based API**
  - All operations return `Result<T>` with structured `Error`
- **ASP.NET Core helpers**
  - `AddHubtelPayments(...)`
  - `MapHubtelReceiveMoneyCallback(...)`

Planned next:

- Background worker for automatic 5-minute status checks
- Example EF Core implementation for pending transactions
- Direct Send Money / Hosted Checkout

---

## 📦 Packages

Planned NuGet packages:

- **`Scynett.Hubtel.Payments`**  
  Core SDK: models, services, HTTP integration.
- **`Scynett.Hubtel.Payments.AspNetCore`**  
  ASP.NET Core integration: DI, minimal APIs, background worker.

---

## 🚀 Getting Started

### 1. Install packages

> (Once published to NuGet – for now, reference the projects directly.)

```bash
dotnet add package Scynett.Hubtel.Payments
dotnet add package Scynett.Hubtel.Payments.AspNetCore
```

### 2. Configure in Program.cs

```csharp
using Scynett.Hubtel.Payments;
using Scynett.Hubtel.Payments.Extensions;
using Scynett.Hubtel.Payments.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHubtelPayments(options =>
{
    options.PosSalesId = "<YOUR_POS_SALES_ID>";
    options.BasicAuthKey = "<YOUR_BASE64_AUTH_KEY>";
    options.PrimaryCallbackUrl = "https://your-api.com/webhooks/hubtel/receive-money";
});

var app = builder.Build();

// Map callback endpoint (Hubtel will POST here)
app.MapHubtelReceiveMoneyCallback("/webhooks/hubtel/receive-money");

app.Run();
```

### 3. Initiate a Receive Money payment

```csharp
using Scynett.Hubtel.Payments.ReceiveMoney;

public class PaymentsController : ControllerBase
{
    private readonly IReceiveMoneyService _receiveMoney;

    public PaymentsController(IReceiveMoneyService receiveMoney)
    {
        _receiveMoney = receiveMoney;
    }

    [HttpPost("api/payments/receive")]
    public async Task<IActionResult> Receive([FromBody] ReceiveMoneyInitRequest request)
    {
        var result = await _receiveMoney.InitiateAsync(request);

        if (!result.IsSuccess)
            return Problem(result.Error.Description, statusCode: 400);

        return Ok(result.Value); // contains ClientReference, TransactionId, etc.
    }
}
```

Example request body:

```json
{
  "customerMsisdn": "233200000000",
  "amount": 10.0,
  "channel": "MtnGh",
  "description": "Order #1234"
}
```

### 4. Handle Hubtel callback

The ASP.NET Core package will register a minimal-API endpoint that deserializes Hubtel’s callback and forwards it to the internal handler.

Typical callback model:

```csharp
using Scynett.Hubtel.Payments.ReceiveMoney;

public class HubtelCallbackHandler
{
    public Task HandleAsync(ReceiveMoneyCallback callback, CancellationToken ct = default)
    {
        if (callback.Status == HubtelPaymentStatus.Success)
        {
            // Mark order as paid, publish domain event, etc.
        }
        else
        {
            // Mark order as failed / cancelled
        }

        return Task.CompletedTask;
    }
}
```

You’ll be able to plug in your own handler implementation via DI.

### 5. Manually check status

```csharp
using Scynett.Hubtel.Payments.Status;

public class PaymentStatusController : ControllerBase
{
    private readonly IHubtelStatusService _statusService;

    public PaymentStatusController(IHubtelStatusService statusService)
    {
        _statusService = statusService;
    }

    [HttpGet("api/payments/{clientReference}/status")]
    public async Task<IActionResult> Get(string clientReference)
    {
        var result = await _statusService.GetStatusAsync(clientReference);

        if (!result.IsSuccess)
            return Problem(result.Error.Description, statusCode: 400);

        return Ok(result.Value);
    }
}
```

---

## 🧱 Persistence & Background Jobs

To fully comply with Hubtel’s “check status after 5 minutes if no callback” rule, you will typically want to store pending transactions.

The core package exposes:

```csharp
public interface IPendingTransactionsStore
{
    Task AddAsync(PendingTransaction tx, CancellationToken ct = default);
    Task<IReadOnlyList<PendingTransaction>> GetDueAsync(CancellationToken ct = default);
    Task MarkCompletedAsync(string clientReference, HubtelPaymentStatus status, CancellationToken ct = default);
}
```

You can provide your own implementation using:

- EF Core (SQL Server / PostgreSQL / SQLite)
- Dapper
- Redis
- Any other store

An optional background worker in Scynett.Hubtel.Payments.AspNetCore will periodically:

1. Fetch due pending transactions
2. Call Hubtel Status API
3. Update their final state via `IPendingTransactionsStore`

---

## 🧬 Internal Architecture

Internally, the library uses:

- CQRS + MediatR style handlers
- Vertical Slice structure per feature (ReceiveMoney.Initiate, ReceiveMoney.Callback, Status.GetStatus)
- HttpClient with proper configuration for Hubtel endpoints
- Result / Error types to avoid exceptions for expected failures

These implementation details are internal and not part of the public API, so we can evolve them without breaking consumers.

---

## 🗺️ Roadmap

- v0.1 – Direct Receive Money + Status Check (core types + ASP.NET helpers)
- v0.2 – Background worker + pending transaction store abstractions
- v0.3 – Direct Send Money
- v0.4 – Hosted Checkout + sample UI (Blazor/MAUI)
- v1.0 – Stable API surface, docs, samples, and NuGet release

---

## 🤝 Contributing

Contributions are welcome!

- Open an issue for bugs, questions, or feature proposals
- Fork the repo and create a PR for improvements
- Use clear commit messages and keep PRs focused

Guidelines and a full contributing document will follow once the core API is stable.

---

## 📝 License

This project will be released under the MIT License (or similar OSI-approved license). License file will be added before the first public NuGet release.

---

If you want, I can next generate:

- The **initial folder + project structure** matching this README
- A **GitHub Actions workflow** to build, test, and (later) publish to NuGet.
