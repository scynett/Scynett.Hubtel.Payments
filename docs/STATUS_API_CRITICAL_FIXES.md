# ?? CRITICAL: Status API Implementation Issues Found

## ?? **Your Status API Implementation is INCORRECT**

### **Issue #1: Wrong API Endpoint**

**Current (WRONG):**
```csharp
var uri = new Uri(
    $"{_options.BaseUrl}/v2/merchantaccount/merchants/{_options.MerchantAccountNumber}/transactions/{query.TransactionId}",
    UriKind.Absolute);
```

**Correct per Hubtel Documentation:**
```csharp
var uri = new Uri(
    $"https://api-txnstatus.hubtel.com/transactions/{_options.MerchantAccountNumber}/status?{queryString}",
    UriKind.Absolute);
```

**API Specification:**
- **Endpoint:** `https://api-txnstatus.hubtel.com/transactions/{POS_Sales_ID}/status`
- **Method:** GET
- **Content-Type:** JSON

---

### **Issue #2: Wrong Query Parameters**

**Current (WRONG):**
```csharp
// Uses TransactionId in URL path
/transactions/{query.TransactionId}
```

**Correct per Hubtel Documentation:**
```
Query parameters (at least one required):
- clientReference (String, Mandatory - preferred)
- hubtelTransactionId (String, Optional)
- networkTransactionId (String, Optional)
```

**Example:**
```
GET https://api-txnstatus.hubtel.com/transactions/12345/status?clientReference=ORD-123
GET https://api-txnstatus.hubtel.com/transactions/12345/status?hubtelTransactionId=HUB-456
GET https://api-txnstatus.hubtel.com/transactions/12345/status?networkTransactionId=NET-789
```

---

### **Issue #3: StatusRequest Model is Wrong**

**Current (WRONG):**
```csharp
public sealed record StatusRequest(string TransactionId);
```

**Correct:**
```csharp
public sealed record StatusRequest
{
    public string? ClientReference { get; init; }
    public string? HubtelTransactionId { get; init; }
    public string? NetworkTransactionId { get; init; }
}
```

---

## ? **Required Changes**

### **1. Update StatusRequest.cs**

```csharp
namespace Scynett.Hubtel.Payments.Features.Status;

/// <summary>
/// Request to check the status of a transaction.
/// You must provide at least one of: ClientReference (preferred), HubtelTransactionId, or NetworkTransactionId.
/// </summary>
public sealed record StatusRequest
{
    /// <summary>
    /// The client reference of the transaction (preferred - mandatory if others not provided).
    /// </summary>
    public string? ClientReference { get; init; }

    /// <summary>
    /// Transaction ID from Hubtel after successful payment (optional).
    /// </summary>
    public string? HubtelTransactionId { get; init; }

    /// <summary>
    /// The transaction reference from the mobile money provider (optional).
    /// </summary>
    public string? NetworkTransactionId { get; init; }

    /// <summary>
    /// Creates a status request using client reference (preferred).
    /// </summary>
    public static StatusRequest ByClientReference(string clientReference) =>
        new() { ClientReference = clientReference };

    /// <summary>
    /// Creates a status request using Hubtel transaction ID.
    /// </summary>
    public static StatusRequest ByHubtelTransactionId(string hubtelTransactionId) =>
        new() { HubtelTransactionId = hubtelTransactionId };

    /// <summary>
    /// Creates a status request using network transaction ID.
    /// </summary>
    public static StatusRequest ByNetworkTransactionId(string networkTransactionId) =>
        new() { NetworkTransactionId = networkTransactionId };
}
```

---

### **2. Update StatusRequestValidator.cs**

**? ALREADY UPDATED** - Changes applied successfully

---

### **3. Update HubtelStatusService.cs**

**Replace the entire CheckStatusAsync method:**

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
        
        // Use Hubtel Status API endpoint (DIFFERENT from payment API!)
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
    // ClientReference is preferred per Hubtel docs
    if (!string.IsNullOrWhiteSpace(query.ClientReference))
        return $"clientReference={Uri.EscapeDataString(query.ClientReference)}";

    if (!string.IsNullOrWhiteSpace(query.HubtelTransactionId))
        return $"hubtelTransactionId={Uri.EscapeDataString(query.HubtelTransactionId)}";

    if (!string.IsNullOrWhiteSpace(query.NetworkTransactionId))
        return $"networkTransactionId={Uri.EscapeDataString(query.NetworkTransactionId)}";

    // This should never happen due to validation, but fallback just in case
    return string.Empty;
}
```

---

## ?? **Manual Steps Required**

Since `HubtelStatusService.cs` is open in your IDE, you need to manually:

### **Step 1: Close the file in Visual Studio**
- Close `HubtelStatusService.cs`

### **Step 2: Edit the file**
Replace the `CheckStatusAsync` method and add the `BuildQueryString` helper method.

### **Step 3: Verify Changes**
Make sure:
1. ? API endpoint is `https://api-txnstatus.hubtel.com/transactions/{POS_Sales_ID}/status`
2. ? Query parameters are used (not path parameters)
3. ? At least one of: clientReference, hubtelTransactionId, or networkTransactionId
4. ? ClientReference is preferred

---

## ?? **Usage Examples**

### ? **Correct Usage (After Fix)**

```csharp
// Preferred: Using ClientReference
var request = StatusRequest.ByClientReference("ORD-123-456");
var result = await statusService.CheckStatusAsync(request);

// Alternative: Using Hubtel Transaction ID
var request = StatusRequest.ByHubtelTransactionId("HUB-789");
var result = await statusService.CheckStatusAsync(request);

// Alternative: Using Network Transaction ID
var request = StatusRequest.ByNetworkTransactionId("NET-456");
var result = await statusService.CheckStatusAsync(request);
```

### ? **Old Usage (Will Break)**

```csharp
// This will NO LONGER WORK after the fix
var request = new StatusRequest("some-transaction-id");
```

---

## ?? **Breaking Changes**

1. **StatusRequest constructor changed** - Now uses properties instead of positional parameter
2. **API endpoint changed** - From `api.hubtel.com` to `api-txnstatus.hubtel.com`
3. **Query mechanism changed** - From path parameter to query string
4. **Multiple query options** - Can now query by ClientReference (preferred), HubtelTransactionId, or NetworkTransactionId

---

## ? **Status of Changes**

| File | Status | Action Required |
|------|--------|-----------------|
| `StatusRequest.cs` | ? **UPDATED** | None - Already fixed |
| `StatusRequestValidator.cs` | ? **UPDATED** | None - Already fixed |
| `HubtelStatusService.cs` | ?? **NEEDS MANUAL UPDATE** | Close file in IDE, then edit |

---

## ?? **Next Steps**

1. ?? **Close `HubtelStatusService.cs` in Visual Studio**
2. ?? **Apply the changes to `HubtelStatusService.cs`**
3. ? **Build and test**
4. ? **Test against Hubtel Sandbox API**

**Priority:** ?? **CRITICAL** - Current implementation will NOT work with Hubtel's actual API!

---

## ?? **API Documentation Reference**

```
API Endpoint: https://api-txnstatus.hubtel.com/transactions/{POS_Sales_ID}/status
Request Type: GET
Content Type: JSON

REQUEST PARAMETERS:
Parameter              Type      Requirement
---------------------------------------------------
clientReference        String    Mandatory (preferred)
hubtelTransactionId    String    Optional
networkTransactionId   String    Optional
```

**Note:** POS_Sales_ID is your `MerchantAccountNumber` from configuration.
