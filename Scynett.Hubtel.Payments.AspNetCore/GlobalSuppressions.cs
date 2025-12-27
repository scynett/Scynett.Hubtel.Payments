using System.Diagnostics.CodeAnalysis;

// CA1848: For simple logging scenarios, LoggerExtensions methods are acceptable
// Converting to LoggerMessage delegates would add significant complexity for minimal benefit
[assembly: SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "LoggerExtensions are acceptable for simple logging scenarios", Scope = "namespaceanddescendants", Target = "~N:Scynett.Hubtel.Payments.AspNetCore")]

// CA1031: Catching general exceptions in background services is acceptable for logging and resilience
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Background services catch all exceptions for resilience and logging", Scope = "namespaceanddescendants", Target = "~N:Scynett.Hubtel.Payments.AspNetCore.Workers")]

// CA1062: Extension method parameters are validated by the framework
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Extension method parameters are validated by the framework", Scope = "namespaceanddescendants", Target = "~N:Scynett.Hubtel.Payments.AspNetCore.Extensions")]

// CA1812: Internal record types used for JSON deserialization are instantiated by System.Text.Json
[assembly: SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Records are instantiated by System.Text.Json during deserialization", Scope = "namespaceanddescendants", Target = "~N:Scynett.Hubtel.Payments.AspNetCore.Endpoints")]
