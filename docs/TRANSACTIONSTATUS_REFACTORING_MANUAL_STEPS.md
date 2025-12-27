# ?? TransactionStatus Refactoring - Manual Steps Required

## ?? **Status: Partial Complete - Manual Intervention Needed**

---

## ? **Completed Steps**

### **1. Model Renamed**
- ? `ReceiveMoneyStatus` ? `TransactionStatus` in `Models/ReceiveMoneyStatus.cs`

### **2. Feature Types Renamed**
- ? `CheckStatusResponse` ? `TransactionStatusResult`
- ? `StatusRequest` ? `TransactionStatusRequest`
- ? `StatusRequestValidator` ? `TransactionStatusRequestValidator`
- ? `HubtelStatusService` ? `TransactionStatusProcessor`

### **3. Namespaces Updated**
- ? All files updated from `.Status` ? `.TransactionStatus`

### **4. Interface Updated**
- ? `ITransactionStatusProcessor` updated to use new types

### **5. ASP.NET Core Integration Updated**
- ? `AspNetCore/Extensions/ServiceCollectionExtensions.cs` updated
- ? `AspNetCore/Workers/PendingTransactionsWorker.cs` updated

---

## ? **Manual Steps Required**

### **Step 1: Close and Update ServiceCollectionExtensions.cs**

**File:** `Scynett.Hubtel.Payments/ServiceCollectionExtensions.cs`

This file is **currently locked/open** in your IDE. You need to:

1. **Close the file** in Visual Studio
2. **Replace line 14** (using statement):

```csharp
// BEFORE
using Scynett.Hubtel.Payments.Features.Status;

// AFTER
using Scynett.Hubtel.Payments.Features.TransactionStatus;
```

3. **Replace line 30** (validator registration):

```csharp
// BEFORE
services.AddScoped<IValidator<StatusRequest>, StatusRequestValidator>();

// AFTER
services.AddScoped<IValidator<TransactionStatusRequest>, TransactionStatusRequestValidator>();
```

4. **Replace line 76** (processor registration):

```csharp
// BEFORE
services.AddScoped<ITransactionStatusProcessor, HubtelStatusService>();

// AFTER
services.AddScoped<ITransactionStatusProcessor, TransactionStatusProcessor>();
```

**Complete Updated File:**

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
using Scynett.Hubtel.Payments.Features.TransactionStatus;  // CHANGED

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
        services.AddScoped<IValidator<TransactionStatusRequest>, TransactionStatusRequestValidator>();  // CHANGED

        // Gateway layer - Refit client for ReceiveMoney with resilience
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
                // Retry configuration with sensible defaults
                // Users can override via HubtelOptions.Resilience in appsettings.json
                resilienceOptions.Retry.MaxRetryAttempts = 3;
                resilienceOptions.Retry.BackoffType = DelayBackoffType.Exponential;
                resilienceOptions.Retry.Delay = TimeSpan.FromSeconds(1);
                resilienceOptions.Retry.UseJitter = true;
                resilienceOptions.Retry.ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .HandleResult(response =>
                        response.StatusCode == HttpStatusCode.RequestTimeout ||
                        response.StatusCode == HttpStatusCode.TooManyRequests ||
                        (int)response.StatusCode >= 500)
                    .Handle<HttpRequestException>()
                    .Handle<TimeoutException>();

                // Circuit breaker configuration
                resilienceOptions.CircuitBreaker.MinimumThroughput = 10;
                resilienceOptions.CircuitBreaker.FailureRatio = 0.5;
                resilienceOptions.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
                resilienceOptions.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
                resilienceOptions.CircuitBreaker.ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .HandleResult(response => (int)response.StatusCode >= 500)
                    .Handle<HttpRequestException>();

                // Timeout configuration
                resilienceOptions.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
                resilienceOptions.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
            });

        // Public API layer - Processors
        services.AddScoped<IReceiveMoneyProcessor, ReceiveMoneyProcessor>();
        services.AddScoped<ITransactionStatusProcessor, TransactionStatusProcessor>();  // CHANGED

        return services;
    }
}
```

---

### **Step 2: Rename Feature Folder**

After fixing ServiceCollectionExtensions.cs:

1. **Close all files** in the Status feature
2. In **Solution Explorer**, rename folder:
   - `Features/Status` ? `Features/TransactionStatus`

This will automatically update file paths and namespaces if done correctly in Visual Studio.

---

### **Step 3: Rename Model File**

In **Solution Explorer**:
- Rename `Models/ReceiveMoneyStatus.cs` ? `Models/TransactionStatus.cs`

---

### **Step 4: Rebuild Solution**

```sh
dotnet clean
dotnet restore
dotnet build
```

Expected: ? **Build Successful**

---

## ?? **Summary of Changes**

### **Types Renamed**

| Before | After | Category |
|--------|-------|----------|
| `ReceiveMoneyStatus` | `TransactionStatus` | Model Enum |
| `HubtelStatusService` | `TransactionStatusProcessor` | Processor |
| `CheckStatusResponse` | `TransactionStatusResult` | Response DTO |
| `StatusRequest` | `TransactionStatusRequest` | Request DTO |
| `StatusRequestValidator` | `TransactionStatusRequestValidator` | Validator |

### **Namespaces Changed**

| Before | After |
|--------|-------|
| `Features.Status` | `Features.TransactionStatus` |

### **Folders to Rename**

| Before | After |
|--------|-------|
| `Features/Status/` | `Features/TransactionStatus/` |
| `Models/ReceiveMoneyStatus.cs` | `Models/TransactionStatus.cs` |

---

## ? **Files Modified** (So Far)

1. ? `Models/ReceiveMoneyStatus.cs` - Type renamed to `TransactionStatus`
2. ? `Features/Status/CheckStatusResponse.cs` - Renamed to `TransactionStatusResult`
3. ? `Features/Status/StatusRequest.cs` - Renamed to `TransactionStatusRequest`
4. ? `Features/Status/StatusRequestValidator.cs` - Renamed to `TransactionStatusRequestValidator`
5. ? `Features/Status/HubtelStatusService.cs` - Renamed to `TransactionStatusProcessor`
6. ? `Features/Status/Log.cs` - Namespace updated to `TransactionStatus`
7. ? `Abstractions/IHubtelStatusService.cs` - Updated to use new types
8. ? `AspNetCore/Extensions/ServiceCollectionExtensions.cs` - Updated
9. ? `AspNetCore/Workers/PendingTransactionsWorker.cs` - Updated

---

## ?? **Recommended Execution Order**

### **Now (Manual Steps):**

1. **Close** `ServiceCollectionExtensions.cs` in VS
2. **Edit** the file manually (3 changes listed above)
3. **Save** the file
4. **Rename** folder: `Features/Status` ? `Features/TransactionStatus`
5. **Rename** file: `Models/ReceiveMoneyStatus.cs` ? `Models/TransactionStatus.cs`
6. **Rebuild** solution

### **Expected Result:**
? Build successful  
? All namespaces consistent  
? No references to old `.Status` namespace

---

## ?? **Why This Refactoring?**

**Problem:** Generic "Status" naming is ambiguous  
**Solution:** Specific "TransactionStatus" naming is clear

**Benefits:**
- ? Clearer intent (transaction status, not just "status")
- ? Better IntelliSense (easier to find)
- ? SDK-appropriate naming
- ? Consistent with other features (ReceiveMoney, TransactionStatus)

---

## ?? **Verification Checklist**

After manual steps:

- [ ] `ServiceCollectionExtensions.cs` uses `TransactionStatus` namespace
- [ ] Folder renamed to `Features/TransactionStatus`
- [ ] File renamed to `Models/TransactionStatus.cs`
- [ ] Solution builds successfully
- [ ] No compilation errors
- [ ] No references to `.Features.Status` namespace remain

---

## ?? **Known Issues (Will Be Resolved After Manual Steps)**

Current build errors:
1. `CS0246: StatusRequest could not be found` ? Fixed by updating ServiceCollectionExtensions.cs
2. `CS0246: StatusRequestValidator could not be found` ? Fixed by updating ServiceCollectionExtensions.cs
3. `CS0246: HubtelStatusService could not be found` ? Fixed by updating ServiceCollectionExtensions.cs
4. Log.cs partial method errors ? Will auto-resolve after rebuild

---

## ?? **Next Steps After Manual Completion**

1. Update documentation (README.md, guides)
2. Update CHANGELOG.md with breaking changes
3. Version bump to v2.1.0 or v3.0.0 (breaking changes)
4. Test all functionality

---

**Estimated Time:** 5-10 minutes for manual steps  
**Impact:** Breaking changes for SDK consumers using Status types
