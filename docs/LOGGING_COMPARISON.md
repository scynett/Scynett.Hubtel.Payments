# Logging Approaches Comparison

## Summary

**We're using Source Generators (`[LoggerMessage]`)** instead of `LoggerMessage.Define` because:

1. ? Cleaner, more maintainable syntax
2. ? Better IDE support and IntelliSense
3. ? Modern .NET standard (recommended by Microsoft for .NET 6+)
4. ? Easier to refactor and maintain
5. ? Optional exception parameters (not always required)

## Side-by-Side Comparison

### Approach 1: Source Generators (What We Use) ?

```csharp
internal static partial class Log
{
    [LoggerMessage(
        EventId = HubtelEventIds.PaymentInitiating,
        Level = LogLevel.Information,
        Message = "Initiating payment for {customerName}")]
    internal static partial void InitiatingPayment(
        ILogger logger,
        string customerName);
}

// Usage
Log.InitiatingPayment(logger, "John Doe");
```

**Pros:**
- Clean, declarative syntax
- Source generator creates implementation at compile time
- Type-safe parameters
- Optional exception parameter
- Better refactoring support
- EventId can be auto-generated or explicit

**Cons:**
- Requires .NET 6+
- Must use `partial` keyword

---

### Approach 2: LoggerMessage.Define (Your Example)

```csharp
public static class LoggerDefinitions
{
    public static readonly Action<ILogger, string, Exception?> PaymentInitiating =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(100, "PaymentInitiating"),
            "Initiating payment for {customerName}");
}

// Usage
LoggerDefinitions.PaymentInitiating(logger, "John Doe", null);
```

**Pros:**
- Works with older .NET versions (.NET Core 2.0+)
- Explicit delegate types visible
- Runtime customization possible
- Can be shared across assemblies easily

**Cons:**
- More verbose syntax
- Must always pass `Exception?` parameter (even if null)
- Manual delegate type matching (`Action<ILogger, T1, T2, Exception?>`)
- Less intuitive to use
- More boilerplate code

---

## Our Hybrid Approach (Best of Both)

We combine Source Generators with centralized EventIds:

```csharp
// 1. Centralized Event IDs (public for SDK consumers)
public static class HubtelEventIds
{
    public const int PaymentInitiating = 100;
    public const int PaymentInitResponse = 101;
    // ...
}

// 2. Feature-specific Log classes (internal)
internal static partial class Log
{
    [LoggerMessage(
        EventId = HubtelEventIds.PaymentInitiating,
        Level = LogLevel.Information,
        Message = "Initiating payment for {customerName}")]
    internal static partial void InitiatingPayment(
        ILogger logger,
        string customerName);
}

// 3. Clean usage in services
Log.InitiatingPayment(_logger, command.CustomerName);
```

**Benefits:**
- ? Clean syntax from Source Generators
- ? Consistent EventIds across SDK
- ? EventIds discoverable by SDK consumers
- ? Type-safe and refactor-friendly
- ? Best performance (zero allocation)
- ? Optional exception handling

---

## Performance Comparison

Both approaches have **identical runtime performance**:
- Zero allocations for logging
- Message templates compiled once
- Same IL generated

The difference is in **developer experience** and **maintainability**.

---

## When to Use Each

### Use Source Generators (`[LoggerMessage]`) When:
- ? .NET 6+ projects
- ? Building modern libraries/SDKs
- ? Want clean, maintainable code
- ? Value IDE support and refactoring

### Use `LoggerMessage.Define` When:
- Supporting .NET Core 2.x / .NET Framework
- Need runtime customization of loggers
- Working in very old codebases

---

## Recommendation for Hubtel.Payments SDK

**Stick with Source Generators** + **Centralized EventIds** because:

1. You're targeting .NET 9
2. It's a modern NuGet package
3. Cleaner code = easier maintenance
4. Better developer experience
5. Microsoft's recommended approach

The old `LoggerDefinitions.cs` file has been removed in favor of the cleaner approach.
