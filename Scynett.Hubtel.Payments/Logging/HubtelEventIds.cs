namespace Scynett.Hubtel.Payments.Logging;

/// <summary>
/// Event IDs for Hubtel Payments logging.
/// </summary>
public static class HubtelEventIds
{
    // Payment Events (100-199)
    public const int PaymentInitiating = 100;
    public const int PaymentInitResponse = 101;
    public const int PaymentInitError = 102;
    public const int TransactionPending = 103;
    public const int TransactionCompleted = 104;

    // Callback Events (200-299)
    public const int CallbackReceived = 200;
    public const int CallbackProcessing = 201;
    public const int CallbackProcessed = 202;
    public const int CallbackError = 203;
    public const int CallbackInvalidData = 204;

    // Status Check Events (300-399)
    public const int StatusCheckStarted = 300;
    public const int StatusCheckCompleted = 301;
    public const int StatusCheckFailed = 302;
    public const int StatusCheckError = 303;

    // Worker Events (400-499)
    public const int WorkerStarted = 400;
    public const int WorkerStopped = 401;
    public const int WorkerCheckingTransactions = 402;
    public const int WorkerNoPendingTransactions = 403;
    public const int WorkerTransactionCheckFailed = 404;
    public const int WorkerError = 405;
    public const int WorkerTransactionError = 406;
}
