namespace Backend.Services;

public sealed class TransactionLogger : ITransactionLogger
{
    private readonly ILogger<TransactionLogger> _logger;

    public TransactionLogger(ILogger<TransactionLogger> logger)
    {
        _logger = logger;
    }

    public string StartTransaction(string module, string action, object? context = null)
    {
        var shortGuid = Guid.NewGuid().ToString("N")[..8];
        var transactionId = $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{shortGuid}";

        _logger.LogInformation(
            "[{Module}] Bắt đầu {Action} | TransactionId={TransactionId} | Context={@Context}",
            module, action, transactionId, context);

        return transactionId;
    }

    public void LogSuccess(string transactionId, string module, string action, object? data = null)
    {
        _logger.LogInformation(
            "[{Module}] {Action} THÀNH CÔNG | TransactionId={TransactionId} | Data={@Data}",
            module, action, transactionId, data);
    }

    public void LogFailure(string transactionId, string module, string action, Exception ex, object? data = null)
    {
        _logger.LogError(ex,
            "[{Module}] {Action} THẤT BẠI | TransactionId={TransactionId} | Data={@Data} | Error={ErrorMessage}",
            module, action, transactionId, data, ex.Message);
    }

    public void LogInterModuleError(string transactionId, string sourceModule, string targetModule, string action, Exception ex)
    {
        _logger.LogError(ex,
            "[GIAO TIẾP LỖI] {SourceModule} -> {TargetModule} khi thực hiện {Action} | TransactionId={TransactionId} | Error={ErrorMessage}",
            sourceModule, targetModule, action, transactionId, ex.Message);
    }
}