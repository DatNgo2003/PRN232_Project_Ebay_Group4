namespace Backend.Services;

/// <summary>
/// Logger dùng chung cho module Payment + Shipping.
/// Mỗi thao tác được gán 1 TransactionId để truy vết xuyên suốt
/// (khởi tạo -> gọi API ngoài -> kết quả), và có hàm riêng để log
/// lỗi giao tiếp giữa 2 module (vd: Order -> Shipping, Payment -> PayPal API).
/// </summary>
public interface ITransactionLogger
{
    string StartTransaction(string module, string action, object? context = null);
    void LogSuccess(string transactionId, string module, string action, object? data = null);
    void LogFailure(string transactionId, string module, string action, Exception ex, object? data = null);
    void LogInterModuleError(string transactionId, string sourceModule, string targetModule, string action, Exception ex);
}