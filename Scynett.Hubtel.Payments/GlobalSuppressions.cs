using System.Diagnostics.CodeAnalysis;

// CA1716: "Error" is a well-established type name in the Result pattern
[assembly: SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Error is a domain-specific type name following the Result pattern", Scope = "type", Target = "~T:Scynett.Hubtel.Payments.Common.Error")]

// CA1056: BaseUrl is stored as string for flexibility and serialization
[assembly: SuppressMessage("Design", "CA1056:URI properties should not be strings", Justification = "BaseUrl is stored as string for configuration flexibility", Scope = "member", Target = "~P:Scynett.Hubtel.Payments.Configuration.HubtelSettings.BaseUrl")]

// CA1031: Catching general exceptions at service boundaries is acceptable for logging and graceful failure
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Service boundary methods catch all exceptions for logging and graceful failure", Scope = "namespaceanddescendants", Target = "~N:Scynett.Hubtel.Payments.Features")]

// CA1062: IOptions and command parameters are validated by the DI container and framework
[assembly: SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Parameters are validated by the DI container and framework", Scope = "namespaceanddescendants", Target = "~N:Scynett.Hubtel.Payments.Features")]

// CA1812: Internal record types used for JSON deserialization are instantiated by System.Text.Json
[assembly: SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Records are instantiated by System.Text.Json during deserialization", Scope = "namespaceanddescendants", Target = "~N:Scynett.Hubtel.Payments.Features")]

// CA1000: Static members on generic types are acceptable for factory methods
[assembly: SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "Static factory methods are appropriate for Result pattern", Scope = "member", Target = "~M:Scynett.Hubtel.Payments.Common.Result`1.FromTValue(`0)~Scynett.Hubtel.Payments.Common.Result{`0}")]
