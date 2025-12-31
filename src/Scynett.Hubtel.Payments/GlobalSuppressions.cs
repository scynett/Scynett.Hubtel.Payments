using System.Diagnostics.CodeAnalysis;

// CA1716: "Error" is a well-established type name in the Result pattern
[assembly: SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Error is a domain-specific type name following the Result pattern", Scope = "type", Target = "~T:Scynett.Hubtel.Payments.Application.Common.Error")]

// CA1716: namespace names align with SDK features (DependencyInjection, DirectReceiveMoney)
[assembly: SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Namespace names describe SDK surface area", Scope = "namespace", Target = "~N:Scynett.Hubtel.Payments.DependencyInjection")]
[assembly: SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Namespace names describe SDK surface area", Scope = "namespace", Target = "~N:Scynett.Hubtel.Payments.DirectReceiveMoney")]

// CA1056: Base addresses are stored as strings for configuration flexibility
[assembly: SuppressMessage("Design", "CA1056:URI properties should not be strings", Justification = "Base addresses are stored as strings for configuration flexibility", Scope = "member", Target = "~P:Scynett.Hubtel.Payments.Options.HubtelOptions.ReceiveMoneyBaseAddress")]
[assembly: SuppressMessage("Design", "CA1056:URI properties should not be strings", Justification = "Base addresses are stored as strings for configuration flexibility", Scope = "member", Target = "~P:Scynett.Hubtel.Payments.Options.HubtelOptions.TransactionStatusBaseAddress")]

// CA1031: Catching general exceptions at service boundaries is acceptable for logging and graceful failure
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Service boundary methods catch all exceptions for logging and graceful failure", Scope = "namespaceanddescendants", Target = "~N:Scynett.Hubtel.Payments.Application.Features")]

// CA1062: IOptions and command parameters are validated by the DI container and framework
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Parameters are validated by the DI container and framework", Scope = "namespaceanddescendants", Target = "~N:Scynett.Hubtel.Payments.Application.Features")]

// CA1062: Extension method parameters are validated by the compiler and caller
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Extension method parameters are guaranteed non-null by the compiler", Scope = "namespaceanddescendants", Target = "~N:Scynett.Hubtel.Payments.Validation")]

// CA1812: Internal classes are instantiated by DI container
[assembly: SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Internal classes are instantiated by dependency injection", Scope = "namespaceanddescendants", Target = "~N:Scynett.Hubtel.Payments.Application.Features")]
[assembly: SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Internal classes are instantiated by dependency injection", Scope = "namespaceanddescendants", Target = "~N:Scynett.Hubtel.Payments.Infrastructure")]

// CA1852: Internal types that don't need inheritance should be sealed
[assembly: SuppressMessage("Performance", "CA1852:Seal internal types", Justification = "Internal types may be extended in future versions", Scope = "namespaceanddescendants", Target = "~N:Scynett.Hubtel.Payments")]

// CA1000: Static members on generic types are acceptable for factory methods
[assembly: SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Static factory methods are appropriate for Result pattern", Scope = "member", Target = "~M:Scynett.Hubtel.Payments.Application.Common.OperationResult`1.FromTValue(`0)~Scynett.Hubtel.Payments.Application.Common.OperationResult{`0}")]



