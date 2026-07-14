# Consumer migration: 0.1.13 → 0.2.0

Packages: `ScynettPayments`, `ScynettPayments.AspNetCore`, `ScynettPayments.Storage.PostgreSql`.

**Nothing in 0.2.0 breaks compilation.** Every change is source- and binary-compatible. What changes is
**behaviour**, in each case because the old behaviour was wrong in a way that can lose money or mis-report a
payment. See [`BREAKING-BEHAVIOUR-CHANGES.md`](./BREAKING-BEHAVIOUR-CHANGES.md) for the rationale behind each
item; this document is the operational checklist.

The unifying principle:

> **"We could not find out what happened" is not the same as "the payment failed."**

---

## TL;DR — what you must actually do

| # | Change | Action required |
| --- | --- | --- |
| 1 | `AddHubtelPayments` now calls `ValidateOnStart()` on `HubtelOptions` | **Verify every environment binds a non-empty `ClientId` and `ClientSecret`.** Otherwise the host will not start. This is the only change that can take an app down. |
| 2 | Unknown response code is no longer `IsFinal` | Stop treating `DirectReceiveMoney.Unknown` as a failed payment. |
| 3 | New error code `DirectReceiveMoney.TransientError` | If you branch on `DirectReceiveMoney.Unknown` for HTTP failures, also handle `DirectReceiveMoney.TransientError`. |
| 4 | Callback endpoint returns 401 / 409 / 500 where it used to return 400 | Review dashboards, alerts, and any WAF/proxy that reacts to 5xx on the callback route. |
| 5 | Failure callbacks with `Amount: 0` are now accepted | Expect previously-stuck pending transactions to start resolving. Make sure your callback handler is safe when it now runs for failure callbacks it never saw before. |
| 6 | Shared-secret compare is constant-time | None. |
| 7 | Channel allowlist widened (`telecel-gh`, `at-gh`, `airteltigo`) | None — legacy `vodafone-gh` / `tigo-gh` still accepted. Widen your *own* allowlist if you have one. |

---

## 1. Startup validation of credentials — the only change that can take you down

`AddHubtelPayments(...)` now registers:

```csharp
services.AddOptions<HubtelOptions>()
    .Validate(o => !string.IsNullOrWhiteSpace(o.ClientId) && !string.IsNullOrWhiteSpace(o.ClientSecret),
              "Hubtel ClientId and ClientSecret must be provided")
    .ValidateOnStart();
```

Previously `AddHubtelOptionsValidation()` was `internal` and called by nothing, so a missing credential was
only discovered when a real customer tried to pay and Hubtel answered `401`.

**Before upgrading, for every environment that calls `AddHubtelPayments` (including CI, test, and any
`WebApplicationFactory` host that boots the real container), confirm both values bind to something non-empty.**

Whatever configuration section you bind `HubtelOptions` from, it must end up with:

```jsonc
{
  "<YourSection>": {
    "ClientId":     "<HUBTEL_CLIENT_ID>",     // must be non-empty
    "ClientSecret": "<HUBTEL_CLIENT_SECRET>"  // must be non-empty
  }
}
```

Equivalent environment variables (double underscore = section separator), e.g.:

```
<YourSection>__ClientId=<HUBTEL_CLIENT_ID>
<YourSection>__ClientSecret=<HUBTEL_CLIENT_SECRET>
```

Gotchas:

- Binding an **empty string** counts as missing (`IsNullOrWhiteSpace`). A deployment that sets
  `...__ClientSecret=""` to "unset" it will now fail to start rather than silently 401 later.
- An **absent** environment variable does *not* clear a value already supplied by a required base JSON file —
  environment variables only override when present with a value.
- Validation runs against the **fully-bound** options, so registration order between your `.Bind(...)` and
  `AddHubtelPayments()` does not matter.

`OptionsValidationExtensions.AddHubtelOptionsValidation()` (in `ScynettPayments.AspNetCore`) is now `public`
and is safe — and unnecessary — to call in addition to `AddHubtelPayments`.

---

## 2 & 3. Unknown and transport failures are no longer final

| Situation | 0.1.13 | 0.2.0 |
| --- | --- | --- |
| Unrecognised Hubtel response code | `Category: Unknown`, `IsFinal: true`, `ErrorType.Failure`, code `DirectReceiveMoney.Unknown` | `Category: Unknown`, **`IsFinal: false`**, `ShouldRetry: true`, `NextAction: RetryLater`, **`ErrorType.Problem`**, code `DirectReceiveMoney.Unknown` (unchanged) |
| Transport failure / non-2xx from Hubtel | fell through to the unknown branch → **final failure**, code `DirectReceiveMoney.Unknown` | `Category: TransientError`, `IsFinal: false`, code **`DirectReceiveMoney.TransientError`**, plus `Error.Metadata["httpStatusCode"]` when Hubtel answered |

`InitiateReceiveMoneyProcessor` still returns a **failed** `OperationResult` in both cases, so
`if (!result.IsSuccess)` keeps working. What changed is *why* it failed and what you should do about it.

The error now carries metadata: `isFinal=false`, `shouldRetry=true`, `nextAction=RetryLater`, and
`httpStatusCode=<n>` on the HTTP path.

**Migration:** do not mark a transaction failed on `DirectReceiveMoney.Unknown` or
`DirectReceiveMoney.TransientError`. Keep it pending and resolve it with a Transaction Status API enquiry.
The customer may well have been debited.

Also: `InvalidOperationException("Hubtel returned empty response body.")` is no longer thrown. If you catch it
by message, remove that. `GatewayInitiateReceiveMoneyResult` gained a trailing optional
`int? HttpStatusCode = null` (source- and binary-compatible).

`ResponseCategory` and `NextAction` gained **no new members** — a `switch` over `ResponseCategory` with no
`default` arm will not silently start dropping cases.

---

## 4. Callback endpoint status codes

| Outcome | 0.1.13 | 0.2.0 |
| --- | --- | --- |
| Processed | 200 | 200 |
| Malformed payload / failed validation (`ErrorType.Validation`) | 400 | 400 |
| Shared-secret / source-IP check failed | 400 | **401** |
| `Hubtel.Callback.InFlight` (`ErrorType.Conflict`) | 400 | **409** |
| Processing failure (`Problem` / `Failure` / anything else) | 400 | **500** |

Response body shape is unchanged: `{ "error": "<code>", "message": "<description>" }`.

Why: a 4xx tells Hubtel "delivered, do not retry". A transient database blip, or a second delivery arriving
while the first was still in flight, used to permanently drop the callback — the payment was never reconciled.

**Migration:**

- Expect 5xx where you previously saw 400. That is Hubtel being correctly asked to retry, not a new fault.
- Review any proxy/WAF/monitoring that alarms or retries on 5xx for the callback route.
- Request-logging that maps `>= 500` to `Error` severity will get noisier for genuinely transient failures.
- The **401** path is only reachable if you enabled `CallbackValidationOptions.EnableValidation`. It is off by
  default; if you never configured it, this row does not apply to you.
- `200` still means "callback processed", **not** "payment succeeded".

> Exceptions thrown by *your* `IReceiveMoneyCallbackHandler` are unchanged: handlers run in the endpoint,
> outside the processor's try/catch, so a throwing handler still escapes to your app's exception middleware.

---

## 5. Failure callbacks with `Amount: 0` are now accepted

`ReceiveMoneyCallbackRequestValidator` required `Data.Amount > 0`. Hubtel sends `Amount: 0` on FAILURE
callbacks, so those were rejected with a 4xx: the pending transaction was never resolved and Hubtel never
retried. The rule is now `>= 0` (negative is still rejected).

**This is the change most likely to produce visible movement in your data.** After upgrading:

- Transactions that were stuck in `Pending` because their failure callback was rejected will now be delivered
  and resolved.
- **Your `IReceiveMoneyCallbackHandler` will now be invoked for failure callbacks it previously never saw.**
  Make sure it is safe in that path — in particular, that it handles a callback whose transaction ID it cannot
  find (a late or unknown callback), rather than dereferencing a missing record and throwing.

Whether a payment succeeded is decided by the **response code**, never by the amount.

---

## 6. Constant-time shared-secret comparison

`string.Equals(..., Ordinal)` → `CryptographicOperations.FixedTimeEquals` over UTF-8 bytes. Correct secrets are
still accepted, wrong secrets still rejected; only the timing profile changes. **No action required.**

Read this even if you change nothing:

> Hubtel **does not sign its callbacks.** There is no HMAC and no digest of the body.
> `CallbackValidationOptions.SignatureHeaderName` and the error code `Hubtel.Callback.InvalidSignature` are
> historical misnomers. All the SDK can check is that the caller echoed back a secret you configured, and
> optionally that the request came from an expected IP. **Neither authenticates the callback body.**
>
> **A callback is a hint, not proof of payment. Verify every transaction against Hubtel's Transaction Status
> API before crediting a wallet, releasing goods, or otherwise treating money as received.**

---

## 7. Channel allowlist

Default accepted channels are now:

| Channel | Note |
| --- | --- |
| `mtn-gh` | current |
| `telecel-gh` | current — replaced `vodafone-gh` |
| `at-gh` | current — replaced `tigo-gh` |
| `airteltigo` | alias used by some Hubtel accounts |
| `vodafone-gh` | legacy, still accepted |
| `tigo-gh` | legacy, still accepted |

Comparison is case-insensitive. Nothing that used to validate stops validating. Override without an SDK release
via `DirectReceiveMoneyOptions.AllowedChannels` (leave unset for the defaults).

**Migration:** none for the SDK. But if *your* application keeps its own channel allowlist, the SDK widening
its list does not widen yours — you must widen yours too, or you will keep rejecting `telecel-gh` / `at-gh`
before the request ever reaches the SDK. Confirm with Hubtel which channel names your merchant account is
actually provisioned for.

---

## Blast radius: ScynettCoreServices (`src\modules\Payments\`)

Assessed against the production consumer at the time of writing. **Verdict: it does not break. It does not even
fail to start. Two behaviours change, both for the better, and one of them needs a code fix to be safe.**

| Hardening change | Effect on ScynettCoreServices |
| --- | --- |
| **`ValidateOnStart` on credentials** | **Unaffected — will not crash.** `PaymentModule.cs:70` binds `AddOptions<HubtelOptions>().Bind(configuration.GetSection("Payments:Providers:Hubtel"))`, and the **required** base file `src\api\Scynett.Core.Api\modules.payments.json` supplies non-empty `ClientId` and `ClientSecret` in every environment (env vars can only override, not clear). The stray `services.Configure<HubtelOptions>(configuration.GetSection(HubtelOptions.SectionName))` at `PaymentModule.cs:94` binds a root `"Hubtel"` section that does not exist — a no-op that does **not** null out the real values. **Only risk:** a deployment that explicitly sets `Payments__Providers__Hubtel__ClientId`/`__ClientSecret` to an **empty string**. Confirm the deployment platform's env vars before rolling out. |
| **Unknown no longer final** | **Initiate path: no change in behaviour.** `HubtelGateway.cs` only does `if (!response.IsSuccess) return Failure(response.Error.ProviderMessage)`. It never reads `Error.Code`, `Error.Type`, or `Error.Metadata`, so a non-final failure still surfaces as a generic failure. `ProviderMessage` is still populated (via `.WithProvider(...)`), so no NRE. **Callback path: no change either** — `PaymentIntentHandler` switches on `result.Category`, and `Unknown` is still `Unknown`; it still transitions to `PaymentStatus.Failed`. ⚠️ **That means the consumer still marks undetermined payments as Failed.** The SDK now tells the truth; the consumer is not yet listening. Recommended follow-up: map `ResponseCategory.Unknown` to `RequiresAction` + status enquiry, as it already does for `TransientError`. |
| **New `DirectReceiveMoney.TransientError` code** | **Unaffected.** Nothing branches on SDK error codes. |
| **`ResponseCategory` / `NextAction` enums** | **Unaffected.** No new members were added, so `PaymentIntentHandler`'s `switch` (which has **no `default` arm**) cannot silently drop a case. |
| **Callback status codes 400 → 401/409/500** | **Behaviour changes, beneficially.** Nothing in the consumer depends on 400; no WAF/proxy/alert rule keys off it. `CallbackValidationOptions` is never configured, so `EnableValidation` is off and the **401 path is unreachable**. The **409** (in-flight) and **500** (transient) changes mean Hubtel will now retry callbacks it previously dropped — the payment gets reconciled instead of hanging. Note `Program.cs` request logging maps `>= 500` to `Error`, so expect some new Error-level log lines that are actually healthy retries. |
| **`Amount: 0` failure callbacks accepted** | ⚠️ **Biggest real-world change, and the one to act on.** Failure callbacks that were previously rejected with a 400 will now be processed, so `PaymentIntentHandler` will run for failure callbacks **it has never been invoked for before**. Expect a wave of `PaymentIntent`s moving `Pending → Failed` and client callbacks firing that never fired. **Action required:** `PaymentIntentHandler` has a latent bug — when `paymentIntentRepository.GetByTransactionIdAsync` returns a failed `Result<PaymentIntent>` it merely does `Console.WriteLine(...)` and falls through to `paymentIntentResult.Value`, which **throws** on a failed `Result`. Because handlers run outside the processor's try/catch, that throw escapes to the API's exception middleware → 500 → Hubtel retries → throws again. Fix that guard (return early) **before** upgrading. |
| **Constant-time secret compare** | **Unaffected.** Callback validation is not enabled. |
| **Channel allowlist widened** | **Unaffected by the SDK change, but the consumer stays broken.** `HubtelChannels.cs` keeps its **own** closed allowlist of `mtn-gh` / `vodafone-gh` / `tigo-gh` and rejects the request in `HubtelGateway.cs` *before* the SDK sees it. Widening the SDK's list does nothing for it. Recommended follow-up: add `telecel-gh`, `at-gh`, `airteltigo` to `HubtelChannels`. |

### Pre-upgrade checklist for ScynettCoreServices

1. **Fix `PaymentIntentHandler`'s failed-`Result` guard** (return early instead of falling through to `.Value`).
   Without this, newly-accepted `Amount: 0` failure callbacks for unknown transaction IDs will throw, and
   Hubtel will now retry them in a loop.
2. **Confirm the deployment platform does not set the Hubtel credential env vars to empty strings.** The
   checked-in base JSON is non-empty, so absent env vars are fine; only an explicit empty override is fatal.
3. Recommended, not required: widen `HubtelChannels` to `telecel-gh` / `at-gh` / `airteltigo`, and stop mapping
   `ResponseCategory.Unknown` to `PaymentStatus.Failed`.
4. Note that the Payments module has **no integration tests and is disabled in CI**, so none of the above will
   be caught before production. Consider a manual smoke test of initiate + callback after upgrading.

---

## Not addressed in 0.2.0 (known, deliberately out of scope)

- `ICallbackAuditStore` has only an **in-memory** implementation — idempotency and replay protection do not
  survive a restart and do not work across instances.
- Dead public types: `IHubtelPayments`, `IHubtelReceiveMoneyClient`, `ResilienceSettings`
  (`HubtelOptions.Resilience` is not the type actually used to configure resilience — see
  `HubtelResilienceOptions`), and the `MyProperty` stubs.
- The test project targets `net10.0` while the libraries target `net9.0`.
