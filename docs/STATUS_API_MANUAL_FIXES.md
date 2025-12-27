# ?? URGENT: Status API Breaking Changes - Manual Fixes Required

## ? Current Status
**Build:** ? **FAILED** (12 compilation errors)  
**Reason:** StatusRequest API was updated but dependent files are open in IDE

## ?? Files That Need Manual Updates

### ? Already Fixed
1. `StatusRequest.cs` - ? Updated with new API
2. `StatusRequestValidator.cs` - ? Updated with validation

### ?? Needs Manual Fix (Files Open in IDE)
1. `HubtelStatusService.cs` - ?? **CRITICAL**
2. `PendingTransactionsWorker.cs` - ?? **HIGH PRIORITY**

---

## ?? Fix #1: HubtelStatusService.cs (CRITICAL)

### **Close the file first, then apply this fix:**

Replace lines 38-92 with:

```csharp
public async Task<Result<CheckStatusResponse>> CheckStatusAsync(
    StatusRequest query,
    CancellationToken cancellationToken = default)
{
    // Validate input
    var validationResult = await _validator.ValidateAsync(query, cancellationToken).ConfigureAwait(false);
    if (!validationResult.IsValid)
    {
        var error = validationResult.ToError();
        var identifier = query.ClientReference ?? query.HubtelTransactionId ?? query.NetworkTransactionId ?? "Unknown";
        Log.ErrorCheckingStatus(_logger, new ValidationException(validationResult.Errors), identifier);
        return Result.Failure<CheckStatusResponse>(error);
    }

    try
    {
        // Set authorization header
        var authValue = Convert.ToBase64String(
            Encoding.ASCII.GetBytes($"{_options.ClientId}:{_options.ClientSecret}"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

        // Build query string based on provided identifier
        var queryString = BuildQueryString(query);
        
        // Use Hubtel Status API endpoint
        var uri = new Uri(
            $"https://api-txnstatus.hubtel.com/transactions/{_options.MerchantAccountNumber}/status?{queryString}",
            UriKind.Absolute);

        var response = await _httpClient.GetAsync(uri, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            Log.FailedToCheckStatus(_logger, response.StatusCode, errorContent);
            return Result.Failure<CheckStatusResponse>(
                new Error("Status.CheckFailed", $"Failed to check status: {response.StatusCode}"));
        }

        var result = await response.Content.ReadFromJsonAsync<HubtelStatusApiResponse>(cancellationToken).ConfigureAwait(false);

        if (result == null || result.Data == null)
        {
            return Result.Failure<CheckStatusResponse>(
                new Error("Status.NullResponse", "Received null response from Hubtel API"));
        }

        return new CheckStatusResponse(
            result.Data.TransactionId ?? string.Empty,
            result.Data.Status ?? string.Empty,
            result.Message ?? string.Empty,
            result.Data.Amount ?? 0,
            result.Data.Charges ?? 0,
            result.Data.CustomerMobileNumber ?? string.Empty);
    }
    catch (Exception ex)
    {
        var identifier = query.ClientReference ?? query.HubtelTransactionId ?? query.NetworkTransactionId ?? "Unknown";
        Log.ErrorCheckingStatus(_logger, ex, identifier);
        return Result.Failure<CheckStatusResponse>(
            new Error("Status.Exception", ex.Message));
    }
}

private static string BuildQueryString(StatusRequest query)
{
    // ClientReference is preferred
    if (!string.IsNullOrWhiteSpace(query.ClientReference))
        return $"clientReference={Uri.EscapeDataString(query.ClientReference)}";

    if (!string.IsNullOrWhiteSpace(query.HubtelTransactionId))
        return $"hubtelTransactionId={Uri.EscapeDataString(query.HubtelTransactionId)}";

    if (!string.IsNullOrWhiteSpace(query.NetworkTransactionId))
        return $"networkTransactionId={Uri.EscapeDataString(query.NetworkTransactionId)}";

    return string.Empty;
}
```

**Key Changes:**
- ? Changed API endpoint to `https://api-txnstatus.hubtel.com`
- ? Use query parameters instead of path parameters
- ? Support multiple query options (ClientReference, HubtelTransactionId, NetworkTransactionId)
- ? Added `BuildQueryString` helper method

---

## ?? Fix #2: PendingTransactionsWorker.cs (HIGH PRIORITY)

### **Line 84: Update StatusRequest constructor**

**BEFORE (Line 84):**
```csharp
var statusResult = await _statusService.CheckStatusAsync(
    new StatusRequest(transactionId),  // ? Wrong
    cancellationToken).ConfigureAwait(false);
```

**AFTER:**
```csharp
var statusResult = await _statusService.CheckStatusAsync(
    StatusRequest.ByHubtelTransactionId(transactionId),  // ? Correct
    cancellationToken).ConfigureAwait(false);
```

**Why:** The worker is checking Hubtel-generated transaction IDs from the pending store, so use `ByHubtelTransactionId`.

---

### **Line 93: Fix Status String Comparison**

The error on line 95 is actually a different issue - appears to be a variable name conflict. Let me check the CheckStatusResponse:

Replace line 93-97 with:

```csharp
var transactionStatus = statusResult.Value.Status.ToUpperInvariant();

if (transactionStatus is "SUCCESS" or "SUCCESSFUL" or "FAILED" or "CANCELLED")
{
    Log.TransactionCompleted(_logger, transactionId, transactionStatus);
```

And line 100-101 with:

```csharp
    var callbackCommand = new PaymentCallback(
        transactionStatus == "SUCCESS" || transactionStatus == "SUCCESSFUL" ? "0000" : "9999",
        transactionStatus,
```

**Complete Fixed Method (CheckPendingTransactionsAsync):**

```csharp
private async Task CheckPendingTransactionsAsync(CancellationToken cancellationToken)
{
    var pendingTransactions = await _pendingStore.GetAllAsync(cancellationToken).ConfigureAwait(false);
    var transactionList = pendingTransactions.ToList();

    if (transactionList.Count == 0)
    {
        Log.NoPendingTransactions(_logger);
        return;
    }

    Log.CheckingPendingTransactions(_logger, transactionList.Count);

    foreach (var transactionId in transactionList)
    {
        if (cancellationToken.IsCancellationRequested)
            break;

        try
        {
            var statusResult = await _statusService.CheckStatusAsync(
                StatusRequest.ByHubtelTransactionId(transactionId),  // ? Fixed
                cancellationToken).ConfigureAwait(false);

            if (statusResult.IsFailure)
            {
                Log.FailedToCheckTransactionStatus(_logger, transactionId, statusResult.Error.Message);
                continue;
            }

            var transactionStatus = statusResult.Value.Status.ToUpperInvariant();  // ? Renamed to avoid conflict

            if (transactionStatus is "SUCCESS" or "SUCCESSFUL" or "FAILED" or "CANCELLED")
            {
                Log.TransactionCompleted(_logger, transactionId, transactionStatus);

                var callbackCommand = new PaymentCallback(
                    transactionStatus == "SUCCESS" || transactionStatus == "SUCCESSFUL" ? "0000" : "9999",
                    transactionStatus,
                    transactionId,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    statusResult.Value.Amount,
                    statusResult.Value.Charges,
                    statusResult.Value.CustomerMobileNumber);

                await _receiveMoneyService.ProcessCallbackAsync(callbackCommand, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            break;
        }
        catch (Exception ex)
        {
            Log.ErrorCheckingTransaction(_logger, ex, transactionId);
        }
    }
}
```

---

## ? Step-by-Step Fix Instructions

### **Step 1: Close All Open Files**
Close these files in Visual Studio:
- `HubtelStatusService.cs`
- `PendingTransactionsWorker.cs`

### **Step 2: Apply Fix to HubtelStatusService.cs**
1. Open `Scynett.Hubtel.Payments/Features/Status/HubtelStatusService.cs`
2. Replace the `CheckStatusAsync` method (lines 38-92)
3. Add the `BuildQueryString` helper method before the record definitions

### **Step 3: Apply Fix to PendingTransactionsWorker.cs**
1. Open `Scynett.Hubtel.Payments.AspNetCore/Workers/PendingTransactionsWorker.cs`
2. Replace line 84: `new StatusRequest(transactionId)` ? `StatusRequest.ByHubtelTransactionId(transactionId)`
3. Replace line 93: `var status` ? `var transactionStatus`
4. Update all references from `status` to `transactionStatus` in lines 95-101

### **Step 4: Build and Verify**
```sh
dotnet restore
dotnet build
```

Expected result: ? **Build successful**

---

## ?? Summary of Changes

| File | Lines Changed | Change Type | Priority |
|------|---------------|-------------|----------|
| `StatusRequest.cs` | Entire file | Breaking API change | ? DONE |
| `StatusRequestValidator.cs` | Entire validator | Breaking validation rules | ? DONE |
| `HubtelStatusService.cs` | 38-92 + new method | API endpoint & logic | ?? TODO |
| `PendingTransactionsWorker.cs` | 84, 93-101 | Constructor & variable | ?? TODO |

---

## ?? Expected Behavior After Fix

### ? Valid Usage
```csharp
// Using client reference (preferred)
var request = StatusRequest.ByClientReference("ORD-123");
var result = await statusService.CheckStatusAsync(request);

// Using Hubtel transaction ID
var request = StatusRequest.ByHubtelTransactionId("HUB-456");
var result = await statusService.CheckStatusAsync(request);

// Using network transaction ID
var request = StatusRequest.ByNetworkTransactionId("NET-789");
var result = await statusService.CheckStatusAsync(request);
```

### ? Old Usage (Will Not Compile)
```csharp
// This no longer works
var request = new StatusRequest("transaction-id");  // ? Compilation error
```

---

## ?? After Fixes Applied

Your Status API will:
1. ? Use correct Hubtel Status API endpoint
2. ? Support multiple query parameter types
3. ? Prefer clientReference per Hubtel recommendation
4. ? Work with actual Hubtel API

**Estimated Time:** 10-15 minutes to apply all fixes

**Next Step:** Test against Hubtel Sandbox!
