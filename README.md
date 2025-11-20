# Scynett.Hubtel.Payments

**Scynett.Hubtel.Payments** is a clean, modern, fully async **.NET SDK** for integrating with the **Hubtel Sales API** – starting with **Direct Receive Money** and **Transaction Status Check**.

It is designed with:

- ✅ **Clean public API** (easy to use, hard to misuse)  
- ✅ **CQRS + Vertical Slice** internally  
- ✅ **Background status checks** (5-minute Hubtel requirement)  
- ✅ **Pluggable persistence** (you decide how/where to store transactions)  
- ✅ **First-class ASP.NET Core support**

> ⚠️ **Status:** Early development (pre-release). API surface may still change.

---

## ✨ Features (v0)

- **Direct Receive Money**
  - Initiate MoMo payments (MTN, Vodafone, AirtelTigo)
  - Strongly-typed request/response models
- **Callbacks**
  - Handle Hubtel’s async callback payload in a single endpoint
- **Status Check**
  - Query final transaction status using `clientReference`
- **Result-based API**
  - All operations return `Result<T>` with structured `Error`
- **ASP.NET Core helpers**
  - `AddHubtelPayments(...)`
  - `MapHubtelReceiveMoneyCallback(...)`

Planned next:

- Background worker for automatic 5-minute status checks  
- Example EF Core implementation for pending transactions  
- Direct Send Money / Hosted Checkout

---

## 📦 Packages

Planned NuGet packages:

- **`Scynett.Hubtel.Payments`**  
  Core SDK: models, services, HTTP integration.

- **`Scynett.Hubtel.Payments.AspNetCore`**  
  ASP.NET Core integration: DI, minimal APIs, background worker.

---

## 🚀 Getting Started

### 1. Install packages

> (Once published to NuGet – for now, reference the projects directly.)

```bash
dotnet add package Scynett.Hubtel.Payments
dotnet add package Scynett.Hubtel.Payments.AspNetCore
