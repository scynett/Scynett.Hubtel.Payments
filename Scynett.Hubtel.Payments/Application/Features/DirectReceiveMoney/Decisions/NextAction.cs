namespace Scynett.Hubtel.Payments.Application.Features.DirectReceiveMoney.Decisions;

public enum NextAction
{
    None,                   // success
    WaitForCallback,        // accepted/pending
    AskCustomerToRetry,     // wrong pin / ussd timeout / temporary user-side issue
    AskCustomerToCheckFunds,// insufficient funds / limits
    FixRequest,             // validation / bad payload
    RetryLater,             // transient / fees not set / service unavailable
    ContactHubtelOrRSE,     // configuration/scopes/fees/channel not enabled
    CheckAuthAndKeys,       // auth mismatch / wrong basic auth / keys mismatch
    NotAllowed              // permission denied for channel
}
