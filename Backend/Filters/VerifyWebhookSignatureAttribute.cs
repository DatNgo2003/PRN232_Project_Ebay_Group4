using Backend.Services;
using Backend.Services.Interface;
using Backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Cryptography;
using System.Text;

namespace Backend.Filters;

/// <summary>
/// Xác thực webhook bằng HMAC-SHA256 trên raw body + timestamp, thay vì so sánh
/// secret trần qua header. Secret KHÔNG đi trên đường truyền mỗi request — chỉ
/// chữ ký (một chiều) mới đi trên wire. Kèm chống replay qua IWebhookReplayGuard.
///
/// Bên gọi (payment/shipping provider) phải ký:
///   signature = HMAC_SHA256(secret, $"{timestamp}.{rawBody}")
/// và gửi header:
///   X-Webhook-Timestamp: <unix seconds>
///   X-Webhook-Signature: <hex lowercase>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class VerifyWebhookSignatureAttribute : Attribute, IAsyncResourceFilter
{
    private readonly string _secretConfigKey;
    private readonly string _moduleName;
    private readonly string _signatureHeader;
    private readonly string _timestampHeader;
    private static readonly TimeSpan MaxClockSkew = TimeSpan.FromMinutes(5);

    public VerifyWebhookSignatureAttribute(
        string secretConfigKey,
        string moduleName,
        string signatureHeader = "X-Webhook-Signature",
        string timestampHeader = "X-Webhook-Timestamp")
    {
        _secretConfigKey = secretConfigKey;
        _moduleName = moduleName;
        _signatureHeader = signatureHeader;
        _timestampHeader = timestampHeader;
    }

    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var http = context.HttpContext;
        var configuration = http.RequestServices.GetRequiredService<IConfiguration>();
        var txLogger = http.RequestServices.GetRequiredService<ITransactionLogger>();
        var replayGuard = http.RequestServices.GetRequiredService<IWebhookReplayGuard>();

        var txId = txLogger.StartTransaction(_moduleName, "Auth", new { Path = http.Request.Path });

        var expectedSecret = configuration[_secretConfigKey];
        if (string.IsNullOrWhiteSpace(expectedSecret))
        {
            txLogger.LogFailure(txId, _moduleName, "Auth",
                new InvalidOperationException($"Config key '{_secretConfigKey}' chưa được thiết lập."), null);
            context.Result = new ObjectResult(new { message = "Webhook chưa được cấu hình trên server." })
            { StatusCode = 500 };
            return;
        }

        var providedSignature = http.Request.Headers[_signatureHeader].ToString();
        var providedTimestampRaw = http.Request.Headers[_timestampHeader].ToString();

        if (string.IsNullOrWhiteSpace(providedSignature) || string.IsNullOrWhiteSpace(providedTimestampRaw))
        {
            txLogger.LogFailure(txId, _moduleName, "Auth",
                new UnauthorizedAccessException("Thiếu header signature hoặc timestamp."), null);
            context.Result = new UnauthorizedObjectResult(new { message = "Missing signature or timestamp header." });
            return;
        }

        if (!long.TryParse(providedTimestampRaw, out var unixSeconds))
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Invalid timestamp header format." });
            return;
        }

        // Chống replay bước 1: request quá cũ hoặc timestamp trong tương lai -> từ chối
        var requestTime = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
        var skew = DateTimeOffset.UtcNow - requestTime;
        if (skew.Duration() > MaxClockSkew)
        {
            txLogger.LogFailure(txId, _moduleName, "Auth",
                new UnauthorizedAccessException("Timestamp ngoài khung cho phép (có thể là replay hoặc đồng hồ lệch)."),
                new { requestTime, skewSeconds = skew.TotalSeconds });
            context.Result = new UnauthorizedObjectResult(new { message = "Request timestamp expired or invalid." });
            return;
        }

        // Đọc raw body để tính chữ ký, rồi reset stream để model binding đọc lại được
        http.Request.EnableBuffering();
        string rawBody;
        using (var reader = new StreamReader(
            http.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true))
        {
            rawBody = await reader.ReadToEndAsync();
        }
        http.Request.Body.Position = 0;

        var signedPayload = $"{providedTimestampRaw}.{rawBody}";
        var expectedSignature = ComputeHmacHex(signedPayload, expectedSecret);

        if (!ConstantTimeCompare.Equals(providedSignature, expectedSignature))
        {
            txLogger.LogFailure(txId, _moduleName, "Auth",
                new UnauthorizedAccessException("Chữ ký webhook không hợp lệ."), null);
            context.Result = new UnauthorizedObjectResult(new { message = "Invalid webhook signature." });
            return;
        }

        // Chống replay bước 2: cùng 1 chữ ký hợp lệ không được xử lý 2 lần
        var eventKey = $"{_moduleName}:{providedSignature}";
        if (!await replayGuard.TryMarkProcessedAsync(eventKey, requestTime))
        {
            txLogger.LogFailure(txId, _moduleName, "Auth",
                new InvalidOperationException("Webhook event đã được xử lý trước đó."), new { eventKey });
            context.Result = new ConflictObjectResult(new { message = "Duplicate webhook event." });
            return;
        }

        txLogger.LogSuccess(txId, _moduleName, "Auth", null);
        http.Items["WebhookAuthTxId"] = txId;

        await next();
    }

    private static string ComputeHmacHex(string payload, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}