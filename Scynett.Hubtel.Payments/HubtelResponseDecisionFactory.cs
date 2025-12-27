using Scynett.Hubtel.Payments.Features.ReceiveMoney.Gateway;

namespace Scynett.Hubtel.Payments;

public static class HubtelResponseDecisionFactory
{
    // Core code mapping (independent of message variants)
    private static readonly Dictionary<string, HandlingDecision> ByCode =
        new Dictionary<string, HandlingDecision>(StringComparer.OrdinalIgnoreCase)
        {
            ["0000"] = new HandlingDecision(
                Code: "0000",
                Description: "The transaction has been processed successfully.",
                NextAction: NextAction.None,
                Category: ResponseCategory.Success,
                IsSuccess: true,
                IsFinal: true,
                ShouldRetry: false,
                CustomerMessage: "Payment successful."),

            ["0001"] = new HandlingDecision(
                Code: "0001",
                Description: "Request has been accepted. A callback will be sent on final state.",
                NextAction: NextAction.WaitForCallback,
                Category: ResponseCategory.Pending,
                IsSuccess: false,
                IsFinal: false,
                ShouldRetry: false,
                CustomerMessage: "Payment request received. Please confirm on your phone.",
                DeveloperHint: "Do not mark as failed. Persist as Pending and await callback/webhook."
            ),

            // 2001 has many message variants, handled below with message-based enrichment.
            ["2001"] = new HandlingDecision(
                Code: "2001",
                Description: "Mobile money customer/rail failure (PIN, funds, limits, timeout, invalid transaction, etc.).",
                NextAction: NextAction.AskCustomerToRetry,
                Category: ResponseCategory.CustomerError,
                IsSuccess: false,
                IsFinal: true,
                ShouldRetry: false,
                CustomerMessage: "Payment could not be completed. Please try again.",
                DeveloperHint: "Use message text to refine: insufficient funds vs wrong PIN vs timeout vs invalid transaction."
            ),

            ["4000"] = new HandlingDecision(
                Code: "4000",
                Description: "Validation errors. Something is not quite right with this request.",
                NextAction: NextAction.FixRequest,
                Category: ResponseCategory.ValidationError,
                IsSuccess: false,
                IsFinal: true,
                ShouldRetry: false,
                CustomerMessage: "We couldn’t start the payment due to invalid details.",
                DeveloperHint: "Inspect request payload; log validation details returned by Hubtel."
            ),

            ["4070"] = new HandlingDecision(
                Code: "4070",
                Description: "Unable to complete payment at the moment. Fees not set for given conditions.",
                NextAction: NextAction.ContactHubtelOrRSE,
                Category: ResponseCategory.ConfigurationError,
                IsSuccess: false,
                IsFinal: true,
                ShouldRetry: true,
                CustomerMessage: "Payment is temporarily unavailable. Please try again later.",
                DeveloperHint: "Ensure minimum amount is passed; otherwise contact Hubtel relationship manager to setup fees."
            ),

            ["4101"] = new HandlingDecision(
                Code: "4101",
                Description: "Business not fully set up / auth scopes mismatch / keys mismatch / POS sales number missing.",
                NextAction: NextAction.CheckAuthAndKeys,
                Category: ResponseCategory.ConfigurationError,
                IsSuccess: false,
                IsFinal: true,
                ShouldRetry: false,
                CustomerMessage: "Payment service is not available for this merchant at the moment.",
                DeveloperHint: "Check Basic Auth, correct API keys, required scopes (e.g., mobilemoney-receive-direct), and POS Sales number."
            ),

            ["4103"] = new HandlingDecision(
                Code: "4103",
                Description: "Permission denied. Account not allowed to transact on this channel.",
                NextAction: NextAction.NotAllowed,
                Category: ResponseCategory.PermissionError,
                IsSuccess: false,
                IsFinal: true,
                ShouldRetry: false,
                CustomerMessage: "This payment method is not available right now.",
                DeveloperHint: "Contact Hubtel Retail Systems Engineer to enable channel permissions."
            ),
        };

    /// <summary>
    /// Main factory method. Provide the Hubtel response code and (optionally) the raw message/description.
    /// The message is used to refine 2001 cases into clearer customer guidance.
    /// </summary>
    public static HandlingDecision Create(string? code, string? message = null)
    {
        code = (code ?? string.Empty).Trim();
        message = Normalize(message);

        if (!ByCode.TryGetValue(code, out var baseDecision))
        {
            return new HandlingDecision(
                Code: string.IsNullOrWhiteSpace(code) ? "UNKNOWN" : code,
                Description: "Unknown response code from Hubtel.",
                NextAction: NextAction.RetryLater,
                Category: ResponseCategory.Unknown,
                IsSuccess: false,
                IsFinal: true,
                ShouldRetry: true,
                CustomerMessage: "We couldn’t complete the payment. Please try again.",
                DeveloperHint: $"Unhandled Hubtel response code '{code}'. Capture and map it. Raw message: '{message ?? ""}'."
            );
        }

        // Refine 2001 variants using message contents.
        if (code.Equals("2001", StringComparison.OrdinalIgnoreCase))
            return Refine2001(baseDecision, message);

        return baseDecision;
    }

    private static HandlingDecision Refine2001(HandlingDecision baseDecision, string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return baseDecision;

        // Funds / limits
        if (ContainsAny(message, "insufficient funds", "avail. balance", "balance limits", "counter", "limit"))
        {
            return baseDecision with
            {
                NextAction = NextAction.AskCustomerToCheckFunds,
                CustomerMessage = "Insufficient funds or account limits reached. Please top up and try again.",
                DeveloperHint = "Customer funds/limits issue (not a system error)."
            };
        }

        // Wrong PIN / invalid PIN
        if (ContainsAny(message, "wrong pin", "invalid pin", "entered the wrong pin", "no or invalid pin", "missing permissions"))
        {
            return baseDecision with
            {
                NextAction = NextAction.AskCustomerToRetry,
                CustomerMessage = "Incorrect PIN. Please try again.",
                DeveloperHint = "Customer entered wrong/invalid PIN."
            };
        }

        // USSD timeout / session timeout
        if (ContainsAny(message, "ussd session timeout", "session timeout", "timeout"))
        {
            return baseDecision with
            {
                NextAction = NextAction.AskCustomerToRetry,
                CustomerMessage = "The confirmation timed out. Please try again and confirm promptly.",
                DeveloperHint = "USSD session timed out."
            };
        }

        // Invalid transaction id
        if (ContainsAny(message, "transaction id is invalid", "invalid transaction"))
        {
            return baseDecision with
            {
                NextAction = NextAction.AskCustomerToRetry,
                CustomerMessage = "Payment could not be verified. Please try again.",
                DeveloperHint = "Invalid/unknown transaction id returned by rail/provider."
            };
        }

        // Network parsing / strange characters
        if (ContainsAny(message, "not able to parse", "strange characters", "(&*!%@)", "parse your request"))
        {
            return baseDecision with
            {
                NextAction = NextAction.FixRequest,
                Category = ResponseCategory.ValidationError,
                CustomerMessage = "We couldn’t start the payment due to invalid details. Please try again.",
                DeveloperHint = "Sanitize/validate description and request fields; avoid special characters in descriptions."
            };
        }

        // Channel mismatch / wrong number
        if (ContainsAny(message, "matches the channel", "number provided matches the channel", "ensure that the number provided matches"))
        {
            return baseDecision with
            {
                NextAction = NextAction.FixRequest,
                Category = ResponseCategory.ValidationError,
                CustomerMessage = "The phone number doesn’t match the selected network. Please correct it and try again.",
                DeveloperHint = "Ensure MSISDN matches channel/network (MTN/Vodafone/AirtelTigo)."
            };
        }

        // FRI not found / account holder issues
        if (ContainsAny(message, "fri not found", "account holder"))
        {
            return baseDecision with
            {
                NextAction = NextAction.AskCustomerToRetry,
                CustomerMessage = "We couldn’t validate this account. Please confirm your number and try again.",
                DeveloperHint = "Provider account holder lookup failed (FRI not found)."
            };
        }

        // Vodafone cash generic failure
        if (ContainsAny(message, "vodafone cash failed", "vodafone failed"))
        {
            return baseDecision with
            {
                NextAction = NextAction.RetryLater,
                Category = ResponseCategory.TransientError,
                ShouldRetry = true,
                CustomerMessage = "Vodafone Cash is currently unavailable. Please try again later.",
                DeveloperHint = "Likely provider-side transient issue."
            };
        }

        // Default fallback: keep base
        return baseDecision with
        {
            CustomerMessage = "Payment could not be completed. Please try again.",
            DeveloperHint = $"2001 variant not specifically mapped. Raw message: '{message}'."
        };
    }

    private static bool ContainsAny(string haystack, params string[] needles)
    {
        foreach (var n in needles)
        {
            if (haystack.Contains(n, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static string? Normalize(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}