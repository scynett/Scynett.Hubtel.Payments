# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.13](https://github.com/scynett/Scynett.Hubtel.Payments/compare/Scynett.Hubtel.Payments-v0.1.12...Scynett.Hubtel.Payments-v0.1.13) (2026-03-11)


### Bug Fixes

* Added Comment to endpoint ([d625f08](https://github.com/scynett/Scynett.Hubtel.Payments/commit/d625f08f5177ccaab235099ae1289e253c55f68f))
* Added Comment to endpoint ([#15](https://github.com/scynett/Scynett.Hubtel.Payments/issues/15)) ([a647c15](https://github.com/scynett/Scynett.Hubtel.Payments/commit/a647c159b32b08a8203a57343f93362a8aa496d9))

## [0.1.12](https://github.com/scynett/Scynett.Hubtel.Payments/compare/Scynett.Hubtel.Payments-v0.1.11...Scynett.Hubtel.Payments-v0.1.12) (2026-03-11)


### Bug Fixes

* Added a Comment on OnCompletedAsync Method ([09630d2](https://github.com/scynett/Scynett.Hubtel.Payments/commit/09630d250bde481fc5452fbc968fa8fde8e3fbfa))
* Added a Comment on OnCompletedAsync Method ([#14](https://github.com/scynett/Scynett.Hubtel.Payments/issues/14)) ([5def7d0](https://github.com/scynett/Scynett.Hubtel.Payments/commit/5def7d0677e0c48c2a065f63d509d597cfb64710))
* Added a Sample project to test the Hubtel Payment Lib ([cb967d5](https://github.com/scynett/Scynett.Hubtel.Payments/commit/cb967d5e05ca41f497f190e55fc0e81d9aaef2c7))
* Added a Sample project to test the Hubtel Payment Lib ([#12](https://github.com/scynett/Scynett.Hubtel.Payments/issues/12)) ([c0192fb](https://github.com/scynett/Scynett.Hubtel.Payments/commit/c0192fbd2b7beec25501787ff5fea223b50aa64d))

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
