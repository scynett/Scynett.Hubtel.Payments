# Breaking behaviour changes — SDK hardening

Read this before upgrading. Nothing below breaks compilation: every change is source- and
binary-compatible for existing callers. What changes is **behaviour** — in each case because the old
behaviour was wrong in a way that can lose money or mis-report a payment.

The unifying principle:

> **"We could not find out what happened" is not the same as "the payment failed."**
> The SDK used to conflate the two. It no longer does.

If your integration relied on any of the old behaviours, the relevant migration note is under each item.

---

## 1. An unknown Hubtel response code is no longer a FINAL FAILURE

**Was:** an unrecognised response code produced `Category: Unknown, IsFinal: true, ShouldRetry: true` —
self-contradictory, and surfaced to the caller as a definitive failure.

**Now:** `Category: Unknown, IsFinal: false, ShouldRetry: true, NextAction: RetryLater`.

**Why it matters:** if Hubtel introduces a code we do not map, the old SDK told you the payment failed —
while the customer had, quite possibly, been debited. That is the "customer paid, app says failed" bug
class. An unknown code now means *go and verify with the Transaction Status API*.

**What you will see:** `InitiateReceiveMoneyProcessor` still returns a **failed** `OperationResult` with
error code `DirectReceiveMoney.Unknown` (unchanged), but the `Error` now carries metadata
`isFinal=false`, `shouldRetry=true`, `nextAction=RetryLater`, and the `ErrorType` is `Problem` rather than
`Failure`.

**Migration:** do not mark the transaction failed on `DirectReceiveMoney.Unknown`. Treat it as
*undetermined*: keep it pending and resolve it with a status enquiry.

---

## 2. A transport failure / non-2xx from Hubtel is now transient, not final

**Was:** the synthetic `HTTP_ERROR` code fell through to the unknown-code branch, so a 5xx from Hubtel,
or a dropped connection, was reported as a final failure.

**Now:** `HTTP_ERROR` maps to `Category: TransientError, IsFinal: false, ShouldRetry: true,
NextAction: RetryLater`.

**What you will see:** a failed `OperationResult` whose error code is **`DirectReceiveMoney.TransientError`**
(previously `DirectReceiveMoney.Unknown`), with `ErrorType.Problem` and metadata `isFinal=false`,
`shouldRetry=true`, `nextAction=RetryLater`, plus **`httpStatusCode`** when Hubtel actually answered.

**Migration:** if you branch on the error code `DirectReceiveMoney.Unknown` for HTTP failures, add
`DirectReceiveMoney.TransientError`. Neither means the payment failed.

---

## 3. Non-2xx responses no longer throw away the HTTP status

**Was:** `HubtelReceiveMoneyGateway` used Refit's `ApiResponse<T>`, which does **not** throw on non-2xx.
A 4xx/5xx therefore arrived with `Content == null` and the gateway threw
`InvalidOperationException("Hubtel returned empty response body.")`. The HTTP status was lost, the
exception was swallowed by the processor and reported as `DirectReceiveMoney.UnhandledException`, and the
`catch (ApiException)` branch was effectively dead code.

**Now:** the gateway checks `IsSuccessStatusCode` / `Error` explicitly and maps any non-2xx, empty body, or
transport failure to the `HTTP_ERROR` transient path, carrying the real status code.

**API addition (source- and binary-compatible):** `GatewayInitiateReceiveMoneyResult` gained a trailing
optional parameter `int? HttpStatusCode = null`. It is populated only on the `HTTP_ERROR` path; it is
`null` on a normal parsed response and on a pure transport failure where no response was received.

**Migration:** none required. `InvalidOperationException("Hubtel returned empty response body.")` will no
longer be thrown — if you catch it by message, remove that. The information now arrives as
`Error.Metadata["httpStatusCode"]`.

---

## 4. Callback endpoint: anything that might succeed on redelivery now returns 5xx

**Was:** every unsuccessful outcome returned **400 Bad Request** — including a processing failure and the
`Hubtel.Callback.InFlight` conflict.

**Why it matters:** a 4xx tells Hubtel "delivered, do not retry". A transient database blip, or a second
delivery arriving while the first was still being processed, permanently dropped the callback: the payment
was never reconciled.

**Now:**

| Outcome | Was | Now |
| --- | --- | --- |
| Processed | 200 | 200 (unchanged) |
| Malformed payload / failed validation (`ErrorType.Validation`) | 400 | 400 (unchanged) |
| Shared-secret / source-IP check failed | 400 | **401** |
| `Hubtel.Callback.InFlight` (`ErrorType.Conflict`) | 400 | **503** |
| Processing failure (`Problem` / `Failure` / anything else) | 400 | **500** |

The response body shape is unchanged: `{ "error": "<code>", "message": "<description>" }`.

**Migration:** if you monitor or alert on the callback endpoint's status codes, expect 5xx to appear where
you previously saw 400 — that is Hubtel being correctly asked to retry, not a new fault. If a proxy or WAF
in front of the endpoint retries or alarms on 5xx, review it. 200 still means "callback processed", **not**
"payment succeeded".

---

## 5. Failure callbacks with `Amount: 0` are now accepted

**Was:** `ReceiveMoneyCallbackRequestValidator` required `Data.Amount > 0`, so a legitimate FAILURE
callback — which Hubtel sends with `Amount: 0` — failed validation and was answered with a 4xx. The pending
transaction was never resolved and Hubtel never retried.

**Now:** the rule is `>= 0`. A negative amount is still rejected. Whether the payment succeeded is decided
by the response code, never by the amount.

**Also:** the no-op rule `RuleFor(x => x.Data!.PaymentDate).Must(_ => true)` was removed. It validated
nothing.

**Migration:** none, unless you asserted that a zero-amount callback is rejected.

---

## 6. Callback shared-secret comparison is now constant-time

**Was:** `string.Equals(..., StringComparison.Ordinal)`, which short-circuits on the first differing
character and therefore leaks the secret to a timing attack.

**Now:** `CryptographicOperations.FixedTimeEquals` over the UTF-8 bytes. Correct secrets are still accepted,
wrong secrets still rejected; only the timing profile changes.

**Read this even if you change nothing:** Hubtel **does not sign its callbacks**. There is no HMAC and no
digest of the body. `CallbackValidationOptions.SignatureHeaderName` and the error code
`Hubtel.Callback.InvalidSignature` are historical misnomers: all the SDK can check is that the caller echoed
back a secret you configured, and optionally that the request came from an expected IP. Neither
authenticates the callback **body**.

> **A callback is a hint, not proof of payment. Verify every transaction against Hubtel's Transaction
> Status API before crediting a wallet, releasing goods, or otherwise treating money as received.**

---

## 7. Missing Hubtel credentials now fail at startup, not at the first payment

**Was:** `AddHubtelOptionsValidation()` was `internal` and called by nothing. An app deployed without
`Hubtel:ClientId` / `Hubtel:ClientSecret` started happily and failed with a **401 from Hubtel on the first
real customer payment**.

**Now:** `AddHubtelPayments(...)` registers eager validation (`ValidateOnStart`). A misconfigured app fails
to start with `Hubtel ClientId and ClientSecret must be provided`.
`OptionsValidationExtensions.AddHubtelOptionsValidation()` is now `public` (in
`ScynettPayments.AspNetCore`) and safe to call explicitly; calling it in addition to `AddHubtelPayments` is
harmless.

**Migration:** make sure every environment that calls `AddHubtelPayments` actually supplies the Hubtel
credentials — including test/CI hosts that previously got away without them. This is the point of the
change: a latent runtime 401 becomes a loud startup failure.

---

## 8. Channel validation accepts Hubtel's current channels

**Was:** `["mtn-gh", "vodafone-gh", "tigo-gh"]` — stale. Vodafone Ghana is now Telecel and Tigo is now
AirtelTigo, so requests using Hubtel's current channel names were rejected by the SDK before they ever
reached Hubtel.

**Now** the default list is:

| Channel | Note |
| --- | --- |
| `mtn-gh` | current |
| `telecel-gh` | current — replaced `vodafone-gh` |
| `at-gh` | current — replaced `tigo-gh` |
| `airteltigo` | current — alias used by some Hubtel accounts |
| `vodafone-gh` | legacy, still accepted |
| `tigo-gh` | legacy, still accepted |

Nothing that used to validate stops validating. Comparison is case-insensitive. The list can be overridden
without an SDK release via `DirectReceiveMoneyOptions.AllowedChannels`; leave it unset to use the defaults.

**Migration:** none. Confirm with Hubtel which channel names your merchant account is actually provisioned
for — the SDK validating a channel does not mean Hubtel accepts it.

---

## Not addressed here (known, deliberately out of scope)

- `ICallbackAuditStore` has only an **in-memory** implementation — idempotency and replay protection do not
  survive a restart and do not work across instances.
- Dead public types: `IHubtelPayments`, `IHubtelReceiveMoneyClient`, `ResilienceSettings`
  (`HubtelOptions.Resilience` is not the type actually used to configure resilience — see
  `HubtelResilienceOptions`), and the `MyProperty` stubs.
- The test project targets `net10.0` while the libraries target `net9.0`.
