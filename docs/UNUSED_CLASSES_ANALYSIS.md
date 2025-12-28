# ?? Unused Classes Analysis - Scynett.Hubtel.Payments

## ?? **Summary**

After examining the solution, I found **several unused/redundant classes** that are **NOT part of the compiled project**. These classes exist in a parallel structure that mirrors the old refactoring attempts.

---

## ? **UNUSED CLASSES - NOT IN BUILD**

These classes are **NOT referenced in the .csproj** and are **NOT being compiled**:

### **Entire Folders NOT in Build:**

```
Scynett.Hubtel.Payments/
??? Features/                           ? ENTIRE FOLDER UNUSED
?   ??? ReceiveMoney/
?   ?   ??? ReceiveMoneyProcessor.cs   ? Duplicate/unused
?   ?   ??? Log.cs                     ? Unused
?   ?   ??? PaymentCallback.cs         ? Duplicate
?   ?   ??? PaymentCallbackValidator.cs ? Duplicate
?   ?   ??? ReceiveMoneyRequest.cs     ? Duplicate
?   ?   ??? ReceiveMoneyResult.cs      ? Duplicate
?   ?   ??? ReceiveMoneyRequestValidator.cs ? Duplicate
?   ?   ??? Gateway/
?   ?       ??? HubtelReceiveMoneyRequest.cs     ? Duplicate
?   ?       ??? HubtelReceiveMoneyResponse.cs    ? Duplicate
?   ?       ??? HubtelReceiveMoneyGateway.cs     ? Duplicate
?   ?       ??? IHubtelReceiveMoneyClient.cs     ? Duplicate
?   ?       ??? IReceiveMobileMoneyService.cs    ? Duplicate
?   ?       ??? HandlingDecision.cs              ? Duplicate
?   ?       ??? NextAction.cs                    ? Duplicate
?   ?       ??? ResponseCategory.cs              ? Duplicate
?   ?
?   ??? Status/ ? TransactionStatus/    ? ENTIRE FOLDER UNUSED
?       ??? TransactionStatusProcessor.cs   ? Duplicate
?       ??? TransactionStatusRequest.cs     ? Duplicate
?       ??? TransactionStatusResult.cs      ? Duplicate
?       ??? TransactionStatusRequestValidator.cs ? Duplicate
?       ??? Log.cs                          ? Unused
?
??? Models/                             ? ENTIRE FOLDER UNUSED
?   ??? ReceiveMoneyStatus.cs ? TransactionState.cs ? Unused
?
??? Common/                             ? ENTIRE FOLDER (if exists)
?   ??? Result.cs                       ? Duplicate
?   ??? Error.cs                        ? Duplicate
?
??? Abstractions/                       ? ENTIRE FOLDER (if exists)
?   ??? IReceiveMoneyService.cs         ? Duplicate
?   ??? IHubtelStatusService.cs         ? Duplicate
?
??? Storage/                            ? ENTIRE FOLDER (if exists)
?   ??? IPendingTransactionsStore.cs    ? Duplicate
?   ??? InMemoryPendingTransactionsStore.cs ? Duplicate
?
??? Configuration/                      ? ENTIRE FOLDER (if exists)
?   ??? HubtelSettings.cs ? HubtelOptions.cs ? Duplicate
?
??? Validation/                         ? ENTIRE FOLDER (if exists)
?   ??? ValidationExtensions.cs         ? Unused
?
??? Logging/                            ? ENTIRE FOLDER (if exists)
    ??? HubtelEventIds.cs               ? Duplicate
```

---

## ? **ACTUALLY USED CLASSES - IN BUILD**

These are the **ONLY** classes being compiled and used:

### **Application Layer**
```
Application/
??? Abstractions/
?   ??? IHubtelReceiveMoneyClient.cs         ? Used in DI
?   ??? IReceiveMobileMoneyService.cs        ? UNUSED - No implementation found
?   ??? IReceiveMoneyProcessor.cs            ? UNUSED - No registration in DI
?   ??? ITransactionStatusProcessor.cs       ? UNUSED - No implementation found
?
??? Common/
?   ??? Error.cs                             ? Used
?   ??? ErrorType.cs                         ? Used
?   ??? OperationResult.cs                   ? Used
?   ??? HubtelEventIds.cs                    ? Used
?   ??? LogMessages.cs                       ? Used
?
??? Features/
    ??? DirectReceiveMoney/
        ??? Decisions/
        ?   ??? HandlingDecision.cs          ? Used
        ?   ??? HubtelResponseDecisionFactory.cs ? Used
        ?   ??? NextAction.cs                ? Used
        ?   ??? ResponseCategory.cs          ? Used
        ?
        ??? Initiate/
            ??? InitiateReceiveMoneyLogMessages.cs    ? Used
            ??? InitiateReceiveMoneyMapping.cs        ? Used
            ??? InitiateReceiveMoneyProcessor.cs      ? Used
            ??? InitiateReceiveMoneyRequest.cs        ? Used
            ??? InitiateReceiveMoneyRequestValidator.cs ? Used
            ??? InitiateReceiveMoneyResult.cs         ? Used
```

### **Infrastructure Layer**
```
Infrastructure/
??? Configuration/
?   ??? HubtelOptions.cs                     ? Used
?   ??? DirectReceiveMoneyOptions.cs         ? Used
?   ??? HubtelResilienceOptions.cs           ? Used
?
??? Http/
?   ??? HubtelAuthHandler.cs                 ? Used
?   ??? HubtelHttpPolicies.cs                ? Used
?   ??? Refit/DirectReceiveMoney/
?       ??? IHubtelDirectReceiveMoneyApi.cs  ? Used
?       ??? Dtos/
?           ??? InitiateReceiveMoneyRequestDto.cs   ? Used
?           ??? InitiateReceiveMoneyResponseDto.cs  ? Used
?           ??? HubtelApiErrorDto.cs                ? Used
?
??? Storage/
    ??? IPendingTransactionsStore.cs         ? Used
    ??? InMemoryPendingTransactionsStore.cs  ? Used
```

### **Public Layer**
```
Public/
??? IHubtelPayments.cs                       ? Used
??? DirectReceiveMoney/
?   ??? IDirectReceiveMoney.cs               ? Used
??? DependencyInjection/
    ??? ServiceCollectionExtensions.cs       ? Used
```

---

## ?? **POTENTIALLY UNUSED - IN BUILD BUT NOT REGISTERED**

These classes exist in the compiled project but **may not be used**:

### **1. Application/Abstractions/IReceiveMobileMoneyService.cs**
```csharp
internal interface IReceiveMobileMoneyService
{
    Task<HubtelReceiveMoneyResponse> InitiateReceiveMoney(
       HubtelReceiveMoneyRequest request,
       CancellationToken cancellationToken = default);
}
```
**Status:** ? **UNUSED**
- No implementation found
- Not registered in DI
- References non-existent types (`HubtelReceiveMoneyRequest`, `HubtelReceiveMoneyResponse`)
- These types don't exist in `Application/Features/`

**Recommendation:** **DELETE**

---

### **2. Application/Abstractions/IReceiveMoneyProcessor.cs**
```csharp
public interface IReceiveMoneyProcessor
{
    Task<Result<ReceiveMoneyResult>> InitAsync(
        ReceiveMoneyRequest request,
        CancellationToken cancellationToken = default);

    Task<Result> ProcessCallbackAsync(
        PaymentCallback callback,
        CancellationToken cancellationToken = default);
}
```
**Status:** ? **UNUSED**
- No implementation in `Application/Features/`
- Not registered in `ServiceCollectionExtensions.cs`
- References non-existent types in Application layer
- Likely leftover from refactoring

**Recommendation:** **DELETE** (or implement if needed)

---

### **3. Application/Abstractions/ITransactionStatusProcessor.cs**
```csharp
public interface ITransactionStatusProcessor
{
    Task<Result<TransactionStatusResult>> CheckStatusAsync(
        TransactionStatusRequest request,
        CancellationToken cancellationToken = default);
}
```
**Status:** ? **UNUSED**
- No implementation found
- No files in `Application/Features/TransactionStatus/`
- Not registered in DI

**Recommendation:** **DELETE** (or implement if needed)

---

### **4. Application/Abstractions/IHubtelReceiveMoneyClient.cs**
```csharp
public interface IHubtelReceiveMoneyClient
{
    [Post("/receive/mobilemoney")]
    Task<HubtelReceiveMoneyResponse> ReceiveMobileMoneyAsync(
        [Body] HubtelReceiveMoneyRequest request,
        CancellationToken cancellationToken = default);
}
```
**Status:** ?? **PARTIALLY USED**
- Registered in `ServiceCollectionExtensions.cs` ?
- But references don't exist: `HubtelReceiveMoneyRequest`, `HubtelReceiveMoneyResponse` ?
- Actual Refit interface should be `IHubtelDirectReceiveMoneyApi`

**Recommendation:** **DELETE** - Use `IHubtelDirectReceiveMoneyApi` instead

---

## ?? **RECOMMENDED CLEANUP ACTIONS**

### **Action 1: Delete Unused Folders (CRITICAL)**

```powershell
# Delete entire folders that are NOT in the build
cd "C:\Workspace\Scynett\Scynett.Hubtel.Payments\Scynett.Hubtel.Payments"

Remove-Item -Path "Features" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "Models" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "Common" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "Abstractions" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "Storage" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "Configuration" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "Validation" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "Logging" -Recurse -Force -ErrorAction SilentlyContinue
```

**Impact:** Removes ~30-50 unused files that are causing confusion

---

### **Action 2: Delete Unused Abstractions (RECOMMENDED)**

```powershell
cd "Application\Abstractions"

# Delete unused interfaces
Remove-Item "IReceiveMobileMoneyService.cs" -Force
Remove-Item "IReceiveMoneyProcessor.cs" -Force
Remove-Item "ITransactionStatusProcessor.cs" -Force
Remove-Item "IHubtelReceiveMoneyClient.cs" -Force
```

**Impact:** Removes 4 unused interface files

---

### **Action 3: Update ServiceCollectionExtensions.cs**

**File:** `Public/DependencyInjection/ServiceCollectionExtensions.cs`

**BEFORE:**
```csharp
services.AddRefitClient<IHubtelReceiveMoneyClient>()
```

**AFTER:**
```csharp
services.AddRefitClient<IHubtelDirectReceiveMoneyApi>()
```

---

## ?? **CLEANUP SUMMARY**

| Category | Count | Action |
|----------|-------|--------|
| **Unused Folders** | 8 folders | ? DELETE |
| **Unused Files (Features/)** | ~30 files | ? DELETE |
| **Unused Abstractions** | 4 interfaces | ? DELETE |
| **Active Classes** | ~35 files | ? KEEP |
| **Potentially Unused** | 0 files | ? All identified |

---

## ? **AFTER CLEANUP**

Your project structure will be clean and match the actual code:

```
Scynett.Hubtel.Payments/
??? Application/              ? Active
?   ??? Common/
?   ??? Features/
?       ??? DirectReceiveMoney/
??? Infrastructure/           ? Active
?   ??? Configuration/
?   ??? Http/
?   ??? Storage/
??? Public/                   ? Active
    ??? DirectReceiveMoney/
    ??? DependencyInjection/
```

**No more duplicate/unused classes causing confusion!**

---

## ?? **EXECUTE CLEANUP**

Run this PowerShell script from the solution root:

```powershell
cd "C:\Workspace\Scynett\Scynett.Hubtel.Payments\Scynett.Hubtel.Payments"

# Backup first (optional but recommended)
git status
git add -A
git commit -m "Backup before cleanup"

# Delete unused folders
$foldersToDelete = @("Features", "Models", "Common", "Abstractions", "Storage", "Configuration", "Validation", "Logging")
foreach ($folder in $foldersToDelete) {
    if (Test-Path $folder) {
        Write-Host "Deleting $folder..." -ForegroundColor Yellow
        Remove-Item -Path $folder -Recurse -Force
    }
}

# Delete unused abstractions
$unusedFiles = @(
    "Application\Abstractions\IReceiveMobileMoneyService.cs",
    "Application\Abstractions\IReceiveMoneyProcessor.cs",
    "Application\Abstractions\ITransactionStatusProcessor.cs",
    "Application\Abstractions\IHubtelReceiveMoneyClient.cs"
)
foreach ($file in $unusedFiles) {
    if (Test-Path $file) {
        Write-Host "Deleting $file..." -ForegroundColor Yellow
        Remove-Item -Path $file -Force
    }
}

# Clean and rebuild
dotnet clean
dotnet restore
dotnet build

Write-Host "`n? Cleanup complete!" -ForegroundColor Green
Write-Host "Review changes with: git status" -ForegroundColor Cyan
```

---

## ?? **EXPECTED RESULT**

After cleanup:
- ? **50+ fewer files**
- ? **No build errors** (should still compile)
- ? **Clear structure** (no duplicates)
- ? **Only active code** in the repository

**Estimated time savings:** 5-10 minutes per refactoring session (no more confusion about which files to modify)
