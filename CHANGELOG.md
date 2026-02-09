# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.11](https://github.com/scynett/Scynett.Hubtel.Payments/compare/Scynett.Hubtel.Payments-v0.1.10...Scynett.Hubtel.Payments-v0.1.11) (2026-02-09)


### Bug Fixes

* correct bash syntax in release workflow ([5b4fb90](https://github.com/scynett/Scynett.Hubtel.Payments/commit/5b4fb9036410a7bd7dddbee4d57b79bb656a9862))
* correct bash syntax in release workflow ([c0a5a13](https://github.com/scynett/Scynett.Hubtel.Payments/commit/c0a5a13b06f2a782caff57405f824dcd2e30ee20))

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
