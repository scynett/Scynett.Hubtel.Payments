# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Initial release of Scynett.Hubtel.Payments SDK
- Support for Hubtel Mobile Money ReceiveMoney operations
- Transaction status checking
- Webhook callback handling
- Built-in resilience with Polly (retry, circuit breaker, timeout)
- High-performance logging with LoggerMessage delegates
- Centralized event IDs for observability
- Result<T> pattern for type-safe error handling
- Background worker for pending transaction polling
- Comprehensive XML documentation
- .NET 9 support

### Features
- **Resilience**: Automatic retry with exponential backoff, circuit breaker, and timeout policies
- **Observability**: Structured logging with event IDs and LoggerMessage delegates
- **Extensibility**: Interface-based design for custom implementations
- **Production-Ready**: Nullable reference types, deterministic builds, and comprehensive error handling

[Unreleased]: https://github.com/scynett/Scynett.Hubtel.Payments/compare/v1.0.0...HEAD
