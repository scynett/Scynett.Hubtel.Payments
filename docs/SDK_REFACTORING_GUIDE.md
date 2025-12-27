# ?? SDK Refactoring Guide: Service ? Processor/Gateway/Client

## ?? **IMPORTANT: Close All Files Before Starting**

Close all files in Visual Studio to avoid file lock issues during refactoring.

---

## ?? **Rename Mapping Summary**

### Configuration
- ? `HubtelSettings` ? `HubtelOptions` (Already created)

### Public API - Processors
- `IReceiveMoneyService` ? `IReceiveMoneyProcessor`
- `IHubtelStatusService` ? `ITransactionStatusProcessor`
- `ReceiveMobileMoneyService` ? `ReceiveMoneyProcessor`
- `HubtelStatusService` ? `TransactionStatusProcessor`

### Gateway Layer - Clients & DTOs
- `IReceiveMoneyApi` ? `IHubtelReceiveMoneyClient`
- `ReceiveMobileMoneyGatewayService` ? `HubtelReceiveMoneyGateway`
- `ReceiveMobileMoneyGatewayRequest` ? `HubtelReceiveMoneyRequest`
- `ReceiveMobileMoneyGatewayResponse` ? `HubtelReceiveMoneyResponse`

### Request/Response Models
- `InitPaymentRequest` ? `ReceiveMoneyRequest`
- `InitPaymentResponse` ? `ReceiveMoneyResult`
- `CheckStatusResponse` ? `TransactionStatusResult`
- `StatusRequest` ? `TransactionStatusRequest`

### Models
- `ReceiveMoneyStatus` ? `TransactionStatus`

### Keep Unchanged
- `IPendingTransactionsStore`
- `InMemoryPendingTransactionsStore`
- `HandlingDecision`
- `NextAction`
- `ResponseCategory`
- `PaymentCallback`
- All validators (will be updated to reference new types)

---

## ?? **Step-by-Step Refactoring Process**

### **Phase 1: Configuration (? Done)**

File already created: `HubtelOptions.cs`

**Action Required:**
1. Delete `Scynett.Hubtel.Payments/Configuration/HubtelSettings.cs`
2. Update all references from `HubtelSettings` ? `HubtelOptions`

---

### **Phase 2: Gateway Layer - DTOs**

#### 2.1 Rename Gateway Request
**File:** `Scynett.Hubtel.Payments/Features/ReceiveMoney/Gateway/ReceiveMobileMoneyGatewayRequest.cs`  
**New Name:** `HubtelReceiveMoneyRequest.cs`

```csharp
namespace Scynett.Hubtel.Payments.Features.ReceiveMoney.Gateway;

/// <summary>
/// Request model for Hubtel Receive Money API.
/// </summary>
public sealed record HubtelReceiveMoneyRequest(
    string CustomerName,
    string CustomerMsisdn,
    string CustomerEmail,
    string Channel,
    string Amount,
    string PrimaryCallbackEndpoint,
    string Description,
    string ClientReference);
```

#### 2.2 Rename Gateway Response
**File:** `Scynett.Hubtel.Payments/Features/ReceiveMoney/Gateway/ReceiveMobileMoneyGatewayResponse.cs`  
**New Name:** `HubtelReceiveMoneyResponse.cs`

```csharp
namespace Scynett.Hubtel.Payments.Features.ReceiveMoney.Gateway;

/// <summary>
/// Response model from Hubtel Receive Money API.
/// </summary>
public sealed record HubtelReceiveMoneyResponse(
    string ResponseCode,
    string Message,
    HubtelReceiveMoneyData? Data);

public sealed record HubtelReceiveMoneyData(
    string? TransactionId,
    string? ClientReference);
```

#### 2.3 Rename Refit Client Interface
**File:** `Scynett.Hubtel.Payments/Features/ReceiveMoney/Gateway/IReceiveMoneyApi.cs`  
**New Name:** `IHubtelReceiveMoneyClient.cs`

```csharp
using Refit;

namespace Scynett.Hubtel.Payments.Features.ReceiveMoney.Gateway;

/// <summary>
/// Refit client for Hubtel Receive Money API.
/// </summary>
public interface IHubtelReceiveMoneyClient
{
    /// <summary>
    /// Initiates a receive money transaction.
    /// </summary>
    [Post("/receive/mobilemoney")]
    Task<HubtelReceiveMoneyResponse> ReceiveMobileMoneyAsync(
        [Body] HubtelReceiveMoneyRequest request,
        CancellationToken cancellationToken = default);
}
```

#### 2.4 Rename Gateway Service
**File:** `Scynett.Hubtel.Payments/Features/ReceiveMoney/Gateway/ReceiveMobileMoneyGatewayService.cs`  
**New Name:** `HubtelReceiveMoneyGateway.cs`

```csharp
using Microsoft.Extensions.Options;

namespace Scynett.Hubtel.Payments.Features.ReceiveMoney.Gateway;

/// <summary>
/// Gateway for interacting with Hubtel Receive Money API.
/// </summary>
internal sealed class HubtelReceiveMoneyGateway
{
    private readonly IHubtelReceiveMoneyClient _client;
    private readonly HubtelOptions _options;

    public HubtelReceiveMoneyGateway(
        IHubtelReceiveMoneyClient client,
        IOptions<HubtelOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    // Methods...
}
```

---

### **Phase 3: Request/Response Models**

#### 3.1 Rename InitPaymentRequest ? ReceiveMoneyRequest
**Directory:** `Scynett.Hubtel.Payments/Features/ReceiveMoney/`  
**Old:** `InitPayment/InitPaymentRequest.cs`  
**New:** `ReceiveMoneyRequest.cs` (move to parent folder)

```csharp
namespace Scynett.Hubtel.Payments.Features.ReceiveMoney;

/// <summary>
/// Request to initiate a receive money transaction.
/// </summary>
public sealed record ReceiveMoneyRequest(
    string? CustomerName,
    string CustomerMobileNumber,
    string Channel,
    decimal Amount,
    string Description,
    string ClientReference,
    string PrimaryCallbackEndPoint);
```

#### 3.2 Rename InitPaymentResponse ? ReceiveMoneyResult
**Old:** `InitPayment/InitPaymentResponse.cs`  
**New:** `ReceiveMoneyResult.cs` (move to parent folder)

```csharp
namespace Scynett.Hubtel.Payments.Features.ReceiveMoney;

/// <summary>
/// Result of a receive money transaction initiation.
/// </summary>
public sealed record ReceiveMoneyResult(
    string TransactionId,
    string CheckoutId,
    string Status,
    string Message);
```

#### 3.3 Rename InitPaymentRequestValidator ? ReceiveMoneyRequestValidator
**Old:** `InitPayment/InitPaymentRequestValidator.cs`  
**New:** `ReceiveMoneyRequestValidator.cs` (move to parent folder)

```csharp
using FluentValidation;

namespace Scynett.Hubtel.Payments.Features.ReceiveMoney;

/// <summary>
/// Validator for ReceiveMoneyRequest based on Hubtel API specifications.
/// </summary>
public sealed class ReceiveMoneyRequestValidator : AbstractValidator<ReceiveMoneyRequest>
{
    // Validation rules...
}
```

#### 3.4 Rename StatusRequest ? TransactionStatusRequest
**File:** `Scynett.Hubtel.Payments/Features/Status/StatusRequest.cs`  
**New Name:** `TransactionStatusRequest.cs`

```csharp
namespace Scynett.Hubtel.Payments.Features.Status;

/// <summary>
/// Request to check the status of a transaction.
/// </summary>
public sealed record TransactionStatusRequest
{
    public string? ClientReference { get; init; }
    public string? HubtelTransactionId { get; init; }
    public string? NetworkTransactionId { get; init; }

    public static TransactionStatusRequest ByClientReference(string clientReference) =>
        new() { ClientReference = clientReference };

    public static TransactionStatusRequest ByHubtelTransactionId(string hubtelTransactionId) =>
        new() { HubtelTransactionId = hubtelTransactionId };

    public static TransactionStatusRequest ByNetworkTransactionId(string networkTransactionId) =>
        new() { NetworkTransactionId = networkTransactionId };
}
```

#### 3.5 Rename CheckStatusResponse ? TransactionStatusResult
**File:** `Scynett.Hubtel.Payments/Features/Status/CheckStatusResponse.cs`  
**New Name:** `TransactionStatusResult.cs`

```csharp
namespace Scynett.Hubtel.Payments.Features.Status;

/// <summary>
/// Result of a transaction status check.
/// </summary>
public sealed record TransactionStatusResult(
    string TransactionId,
    string Status,
    string Message,
    decimal Amount,
    decimal Charges,
    string CustomerMobileNumber);
```

#### 3.6 Rename StatusRequestValidator ? TransactionStatusRequestValidator
**File:** `Scynett.Hubtel.Payments/Features/Status/StatusRequestValidator.cs`  
**New Name:** `TransactionStatusRequestValidator.cs`

```csharp
using FluentValidation;

namespace Scynett.Hubtel.Payments.Features.Status;

/// <summary>
/// Validator for TransactionStatusRequest.
/// </summary>
public sealed class TransactionStatusRequestValidator : AbstractValidator<TransactionStatusRequest>
{
    // Validation rules...
}
```

---

### **Phase 4: Public API - Processor Interfaces**

#### 4.1 Rename IReceiveMoneyService ? IReceiveMoneyProcessor
**File:** `Scynett.Hubtel.Payments/Abstractions/IReceiveMoneyService.cs`  
**New Name:** `IReceiveMoneyProcessor.cs`

```csharp
using Scynett.Hubtel.Payments.Common;
using Scynett.Hubtel.Payments.Features.ReceiveMoney;

namespace Scynett.Hubtel.Payments.Abstractions;

/// <summary>
/// Processor for Hubtel receive money operations.
/// </summary>
public interface IReceiveMoneyProcessor
{
    /// <summary>
    /// Initiates a receive money transaction.
    /// </summary>
    Task<Result<ReceiveMoneyResult>> InitAsync(
        ReceiveMoneyRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes a payment callback from Hubtel.
    /// </summary>
    Task<Result> ProcessCallbackAsync(
        PaymentCallback callback,
        CancellationToken cancellationToken = default);
}
```

#### 4.2 Rename IHubtelStatusService ? ITransactionStatusProcessor
**File:** `Scynett.Hubtel.Payments/Abstractions/IHubtelStatusService.cs`  
**New Name:** `ITransactionStatusProcessor.cs`

```csharp
using Scynett.Hubtel.Payments.Common;
using Scynett.Hubtel.Payments.Features.Status;

namespace Scynett.Hubtel.Payments.Abstractions;

/// <summary>
/// Processor for checking Hubtel transaction status.
/// </summary>
public interface ITransactionStatusProcessor
{
    /// <summary>
    /// Checks the status of a transaction.
    /// </summary>
    Task<Result<TransactionStatusResult>> CheckStatusAsync(
        TransactionStatusRequest request,
        CancellationToken cancellationToken = default);
}
```

---

### **Phase 5: Processor Implementations**

#### 5.1 Rename ReceiveMobileMoneyService ? ReceiveMoneyProcessor
**File:** `Scynett.Hubtel.Payments/Features/ReceiveMoney/ReceiveMobileMoneyService.cs`  
**New Name:** `ReceiveMoneyProcessor.cs`

```csharp
using FluentValidation;
using Microsoft.Extensions.Logging;
using Scynett.Hubtel.Payments.Abstractions;
using Scynett.Hubtel.Payments.Common;
using Scynett.Hubtel.Payments.Features.ReceiveMoney.Gateway;
using Scynett.Hubtel.Payments.Storage;
using Scynett.Hubtel.Payments.Validation;
using System.Globalization;

namespace Scynett.Hubtel.Payments.Features.ReceiveMoney;

/// <summary>
/// Processor for Hubtel receive money operations.
/// </summary>
public sealed class ReceiveMoneyProcessor(
    IHubtelReceiveMoneyClient client,
    IPendingTransactionsStore pendingStore,
    ILogger<ReceiveMoneyProcessor> logger,
    IValidator<ReceiveMoneyRequest> requestValidator,
    IValidator<PaymentCallback> callbackValidator) : IReceiveMoneyProcessor
{
    public async Task<Result<ReceiveMoneyResult>> InitAsync(
        ReceiveMoneyRequest request,
        CancellationToken cancellationToken = default)
    {
        // Implementation...
    }

    public async Task<Result> ProcessCallbackAsync(
        PaymentCallback callback,
        CancellationToken cancellationToken = default)
    {
        // Implementation...
    }
}
```

#### 5.2 Rename HubtelStatusService ? TransactionStatusProcessor
**File:** `Scynett.Hubtel.Payments/Features/Status/HubtelStatusService.cs`  
**New Name:** `TransactionStatusProcessor.cs`

```csharp
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Scynett.Hubtel.Payments.Abstractions;
using Scynett.Hubtel.Payments.Common;
using Scynett.Hubtel.Payments.Configuration;
using Scynett.Hubtel.Payments.Validation;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace Scynett.Hubtel.Payments.Features.Status;

/// <summary>
/// Processor for checking Hubtel transaction status.
/// </summary>
public sealed class TransactionStatusProcessor : ITransactionStatusProcessor
{
    private readonly HttpClient _httpClient;
    private readonly HubtelOptions _options;
    private readonly ILogger<TransactionStatusProcessor> _logger;
    private readonly IValidator<TransactionStatusRequest> _validator;

    public TransactionStatusProcessor(
        HttpClient httpClient,
        IOptions<HubtelOptions> options,
        ILogger<TransactionStatusProcessor> logger,
        IValidator<TransactionStatusRequest> validator)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        _validator = validator;
    }

    public async Task<Result<TransactionStatusResult>> CheckStatusAsync(
        TransactionStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        // Implementation...
    }
}
```

---

### **Phase 6: Models**

#### 6.1 Rename ReceiveMoneyStatus ? TransactionStatus
**File:** `Scynett.Hubtel.Payments/Models/ReceiveMoneyStatus.cs`  
**New Name:** `TransactionStatus.cs`

```csharp
namespace Scynett.Hubtel.Payments.Models;

/// <summary>
/// Represents the status of a Hubtel transaction.
/// </summary>
public enum TransactionStatus
{
    Pending,
    Success,
    Failed,
    Cancelled
}
```

---

### **Phase 7: Update ServiceCollectionExtensions**

**File:** `Scynett.Hubtel.Payments/ServiceCollectionExtensions.cs`

```csharp
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;
using Refit;
using Scynett.Hubtel.Payments.Abstractions;
using Scynett.Hubtel.Payments.Configuration;
using Scynett.Hubtel.Payments.Features.ReceiveMoney;
using Scynett.Hubtel.Payments.Features.ReceiveMoney.Gateway;
using Scynett.Hubtel.Payments.Features.Status;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Scynett.Hubtel.Payments;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHubtelPaymentsCore(this IServiceCollection services)
    {
        // Validators
        services.AddScoped<IValidator<ReceiveMoneyRequest>, ReceiveMoneyRequestValidator>();
        services.AddScoped<IValidator<PaymentCallback>, PaymentCallbackValidator>();
        services.AddScoped<IValidator<TransactionStatusRequest>, TransactionStatusRequestValidator>();

        // Gateway - Refit client for ReceiveMoney
        services.AddRefitClient<IHubtelReceiveMoneyClient>()
            .ConfigureHttpClient((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<HubtelOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

                var authValue = Convert.ToBase64String(
                    Encoding.ASCII.GetBytes($"{options.ClientId}:{options.ClientSecret}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
            })
            .AddStandardResilienceHandler(resilienceOptions =>
            {
                // Resilience configuration...
            });

        // Processors
        services.AddScoped<IReceiveMoneyProcessor, ReceiveMoneyProcessor>();
        services.AddScoped<ITransactionStatusProcessor, TransactionStatusProcessor>();

        return services;
    }
}
```

---

### **Phase 8: Update ASP.NET Core Extensions**

**File:** `Scynett.Hubtel.Payments.AspNetCore/Extensions/ServiceCollectionExtensions.cs`

Update:
- `IReceiveMoneyService` ? `IReceiveMoneyProcessor`
- `HubtelSettings` ? `HubtelOptions`

---

### **Phase 9: Update ASP.NET Core Endpoints & Workers**

**Files to update:**
- `Scynett.Hubtel.Payments.AspNetCore/Endpoints/HubtelCallbackEndpoints.cs`
- `Scynett.Hubtel.Payments.AspNetCore/Workers/PendingTransactionsWorker.cs`

Change references:
- `IReceiveMoneyService` ? `IReceiveMoneyProcessor`
- `IHubtelStatusService` ? `ITransactionStatusProcessor`
- `StatusRequest` ? `TransactionStatusRequest`

---

### **Phase 10: Delete Old Files & Cleanup**

After all renames and updates, delete:
- `HubtelSettings.cs`
- `InitPayment/` folder (after moving files)
- Old interface/implementation files

---

## ? **Verification Checklist**

After all changes:

1. **Build Success**
   ```sh
   dotnet clean
   dotnet restore
   dotnet build
   ```

2. **Public API Stability**
   - ? `IReceiveMoneyProcessor` exists and is public
   - ? `ITransactionStatusProcessor` exists and is public
   - ? `ReceiveMoneyRequest` is public
   - ? `ReceiveMoneyResult` is public
   - ? `TransactionStatusRequest` is public
   - ? `TransactionStatusResult` is public

3. **Namespaces Unchanged**
   - ? All types remain in same namespaces
   - ? No breaking changes to public API surface

4. **XML Documentation**
   - ? All public types have XML docs
   - ? Docs reflect new names

5. **Functionality**
   - ? All tests pass (if you have tests)
   - ? No runtime errors

---

## ?? **Breaking Changes for Consumers**

This refactoring introduces **breaking changes** for SDK consumers:

### **v1.x ? v2.0 Migration Guide**

```csharp
// OLD (v1.x)
using Scynett.Hubtel.Payments.Abstractions;
using Scynett.Hubtel.Payments.Features.ReceiveMoney;

services.Configure<HubtelSettings>(config.GetSection("Hubtel"));
services.AddHubtelPaymentsCore();

var service = serviceProvider.GetRequiredService<IReceiveMoneyService>();
var request = new InitPaymentRequest(...);
var result = await service.InitAsync(request);

// NEW (v2.0)
using Scynett.Hubtel.Payments.Abstractions;
using Scynett.Hubtel.Payments.Features.ReceiveMoney;

services.Configure<HubtelOptions>(config.GetSection("Hubtel"));  // Changed
services.AddHubtelPaymentsCore();

var processor = serviceProvider.GetRequiredService<IReceiveMoneyProcessor>();  // Changed
var request = new ReceiveMoneyRequest(...);  // Changed
var result = await processor.InitAsync(request);
```

---

## ?? **Recommended Approach**

Given the scope of this refactoring, I recommend:

1. **Create a new branch:** `git checkout -b refactor/sdk-naming`
2. **Execute refactoring systematically** (use IDE refactoring tools when possible)
3. **Build after each phase** to catch errors early
4. **Commit after each successful phase**
5. **Test thoroughly** before merging

---

## ?? **Estimated Time**

- **Manual Refactoring:** 2-3 hours
- **Testing & Verification:** 1 hour
- **Total:** 3-4 hours

---

## ?? **Priority Order for Manual Execution**

If doing this manually in Visual Studio:

1. Use **Rename Symbol** (F2) for:
   - `HubtelSettings` ? `HubtelOptions`
   - `IReceiveMoneyService` ? `IReceiveMoneyProcessor`
   - `IHubtelStatusService` ? `ITransactionStatusProcessor`

2. Use **File Rename** + **Find/Replace** for:
   - Gateway DTOs
   - Request/Response models

3. **Manually update** ServiceCollectionExtensions

This guide provides the complete blueprint. Would you like me to execute specific phases, or would you prefer to do this manually using your IDE's refactoring tools?
