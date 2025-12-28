using System.Diagnostics.CodeAnalysis;

// CA1716: "Error" is a well-established type name in the Result pattern
[assembly: SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Error is a domain-specific type name following the Result pattern", Scope = "type", Target = "~T:Scynett.Hubtel.Payments.Application.Common.Error")]

// CA1716: "Public" namespace is intentional for public SDK APIs
[assembly: SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Public namespace clearly indicates public SDK surface area", Scope = "namespace", Target = "~N:Scynett.Hubtel.Payments.Public")]
[assembly: SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Public namespace clearly indicates public SDK surface area", Scope = "namespace", Target = "~N:Scynett.Hubtel.Payments.Public.DependencyInjection")]
[assembly: SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Public namespace clearly indicates public SDK surface area", Scope = "namespace", Target = "~N:Scynett.Hubtel.Payments.Public.DirectReceiveMoney")]

// CA1056: BaseUrl is stored as string for flexibility and serialization
[assembly: SuppressMessage("Design", "CA1056:URI properties should not be strings", Justification = "BaseUrl is stored as string for configuration flexibility", Scope = "member", Target = "~P:Scynett.Hubtel.Payments.Infrastructure.Configuration.HubtelOptions.BaseUrl")]

// CA1031: Catching general exceptions at service boundaries is acceptable for logging and graceful failure
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Service boundary methods catch all exceptions for logging and graceful failure", Scope = "namespaceanddescendants", Target = "~N:Scynett.Hubtel.Payments.Application.Features")]

// CA1062: IOptions and command parameters are validated by the DI container and framework
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Parameters are validated by the DI container and framework", Scope = "namespaceanddescendants", Target = "~N:Scynett.Hubtel.Payments.Application.Features")]

// CA1062: Extension method parameters are validated by the compiler and caller
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Extension method parameters are guaranteed non-null by the compiler", Scope = "namespaceanddescendants", Target = "~N:Scynett.Hubtel.Payments.Validation")]

// CA1812: Internal classes are instantiated by DI container
[assembly: SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Internal classes are instantiated by dependency injection", Scope = "namespaceanddescendants", Target = "~N:Scynett.Hubtel.Payments.Application.Features")]
[assembly: SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Internal classes are instantiated by dependency injection", Scope = "namespaceanddescendants", Target = "~N:Scynett.Hubtel.Payments.Infrastructure")]
[assembly: SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Internal classes are instantiated by dependency injection", Scope = "namespaceanddescendants", Target = "~N:Scynett.Hubtel.Payments.Public")]

// CA1852: Internal types that don't need inheritance should be sealed
[assembly: SuppressMessage("Performance", "CA1852:Seal internal types", Justification = "Internal types may be extended in future versions", Scope = "namespaceanddescendants", Target = "~N:Scynett.Hubtel.Payments")]

// CA1000: Static members on generic types are acceptable for factory methods
[assembly: SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Static factory methods are appropriate for Result pattern", Scope = "member", Target = "~M:Scynett.Hubtel.Payments.Application.Common.OperationResult`1.FromTValue(`0)~Scynett.Hubtel.Payments.Application.Common.OperationResult{`0}")]
