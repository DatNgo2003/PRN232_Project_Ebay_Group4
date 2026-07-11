using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Backend.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        private string SmtpHost => _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        private int SmtpPort => _configuration.GetValue<int>("Email:SmtpPort", 587);
        private string FromEmail => _configuration["Email:FromEmail"] ?? "";
        private string FromName => _configuration["Email:FromName"] ?? "eBay Clone Shop";
        private string Password => _configuration["Email:Password"] ?? "";

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendShippingStatusEmailAsync(
            string toEmail,
            string buyerName,
            int orderId,
            string shippingStatus,
            IEnumerable<string> productNames,
            string trackingNumber)
        {
            bool isDelivered = shippingStatus.Equals("Delivered", StringComparison.OrdinalIgnoreCase);
            var subject = isDelivered
                ? $"✅ Đơn hàng #{orderId} đã được giao thành công!"
                : $"❌ Giao hàng thất bại - Đơn hàng #{orderId}";

            var statusColor = isDelivered ? "#10b981" : "#ef4444";
            var statusIcon = isDelivered ? "✅" : "❌";
            var statusTitle = isDelivered ? "Giao hàng thành công" : "Giao hàng thất bại";
            var statusMessage = isDelivered
                ? "Đơn hàng của bạn đã được giao đến tay bạn thành công. Cảm ơn bạn đã mua sắm tại eBay Clone!"
                : "Rất tiếc, đơn hàng của bạn không thể giao thành công. Chúng tôi sẽ liên hệ để sắp xếp lại việc giao hàng.";

            var productList = string.Join("", productNames.Select(p =>
                $"<li style=\"padding:6px 0; border-bottom:1px solid #f3f4f6; color:#374151;\">{System.Net.WebUtility.HtmlEncode(p)}</li>"));

            var body = BuildEmailHtml(
                buyerName: buyerName,
                statusColor: statusColor,
                statusIcon: statusIcon,
                statusTitle: statusTitle,
                statusMessage: statusMessage,
                orderId: orderId,
                productList: productList,
                extraInfo: $"<p style=\"margin:0; color:#6b7280;\"><strong>Mã vận đơn:</strong> {System.Net.WebUtility.HtmlEncode(trackingNumber)}</p>");

            await SendEmailAsync(toEmail, buyerName, subject, body);
        }

        public async Task SendOrderCancelledEmailAsync(
            string toEmail,
            string buyerName,
            int orderId,
            IEnumerable<string> productNames,
            decimal totalAmount)
        {
            var subject = $"🚫 Đơn hàng #{orderId} đã bị huỷ do quá hạn thanh toán";

            var productList = string.Join("", productNames.Select(p =>
                $"<li style=\"padding:6px 0; border-bottom:1px solid #f3f4f6; color:#374151;\">{System.Net.WebUtility.HtmlEncode(p)}</li>"));

            var body = BuildEmailHtml(
                buyerName: buyerName,
                statusColor: "#f59e0b",
                statusIcon: "🚫",
                statusTitle: "Đơn hàng đã bị huỷ",
                statusMessage: "Đơn hàng của bạn đã bị tự động huỷ do không thanh toán trong thời gian quy định. Bạn có thể đặt lại đơn hàng bất kỳ lúc nào.",
                orderId: orderId,
                productList: productList,
                extraInfo: $"<p style=\"margin:0; color:#6b7280;\"><strong>Tổng tiền:</strong> ${totalAmount:F2}</p>");

            await SendEmailAsync(toEmail, buyerName, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string toName, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(FromEmail) || FromEmail == "your-email@gmail.com")
            {
                _logger.LogWarning("Email chưa được cấu hình. Bỏ qua gửi email tới {ToEmail}", toEmail);
                return;
            }

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(FromName, FromEmail));
                message.To.Add(new MailboxAddress(toName, toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(SmtpHost, SmtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(FromEmail, Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email đã gửi thành công tới {ToEmail} - Subject: {Subject}", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gửi email tới {ToEmail}", toEmail);
                // Không throw để không ảnh hưởng luồng chính
            }
        }

        private static string BuildEmailHtml(
            string buyerName,
            string statusColor,
            string statusIcon,
            string statusTitle,
            string statusMessage,
            int orderId,
            string productList,
            string extraInfo)
        {
            return $@"
<!DOCTYPE html>
<html lang=""vi"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
</head>
<body style=""margin:0; padding:0; background-color:#f9fafb; font-family:'Segoe UI', Arial, sans-serif;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f9fafb; padding:40px 20px;"">
    <tr>
      <td align=""center"">
        <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background:#ffffff; border-radius:12px; overflow:hidden; box-shadow:0 4px 20px rgba(0,0,0,0.08);"">
          
          <!-- Header -->
          <tr>
            <td style=""background:linear-gradient(135deg,#1e3a5f 0%,#2563eb 100%); padding:32px 40px; text-align:center;"">
              <h1 style=""margin:0; color:#ffffff; font-size:26px; font-weight:700; letter-spacing:-0.5px;"">🛍️ eBay Clone</h1>
              <p style=""margin:6px 0 0; color:#bfdbfe; font-size:13px;"">Thông báo đơn hàng</p>
            </td>
          </tr>

          <!-- Status Banner -->
          <tr>
            <td style=""background:{statusColor}; padding:20px 40px; text-align:center;"">
              <p style=""margin:0; color:#ffffff; font-size:22px; font-weight:700;"">{statusIcon} {statusTitle}</p>
            </td>
          </tr>

          <!-- Body -->
          <tr>
            <td style=""padding:36px 40px;"">
              <p style=""margin:0 0 20px; color:#111827; font-size:16px;"">Xin chào <strong>{System.Net.WebUtility.HtmlEncode(buyerName)}</strong>,</p>
              <p style=""margin:0 0 28px; color:#4b5563; font-size:15px; line-height:1.7;"">{statusMessage}</p>

              <!-- Order Info Card -->
              <div style=""background:#f8fafc; border:1px solid #e2e8f0; border-radius:8px; padding:20px 24px; margin-bottom:24px;"">
                <p style=""margin:0 0 10px; color:#6b7280; font-size:13px; text-transform:uppercase; letter-spacing:0.5px; font-weight:600;"">Thông tin đơn hàng</p>
                <p style=""margin:0 0 8px; color:#111827; font-size:16px;""><strong>Mã đơn hàng: #{orderId}</strong></p>
                {extraInfo}
              </div>

              <!-- Products -->
              <div style=""margin-bottom:28px;"">
                <p style=""margin:0 0 10px; color:#6b7280; font-size:13px; text-transform:uppercase; letter-spacing:0.5px; font-weight:600;"">Sản phẩm</p>
                <ul style=""margin:0; padding:0; list-style:none; background:#f8fafc; border:1px solid #e2e8f0; border-radius:8px; padding:8px 16px;"">
                  {productList}
                </ul>
              </div>

              <p style=""margin:0; color:#6b7280; font-size:14px; line-height:1.6;"">
                Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với chúng tôi qua email hoặc hệ thống chat trực tiếp.
              </p>
            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td style=""background:#f1f5f9; padding:20px 40px; text-align:center; border-top:1px solid #e2e8f0;"">
              <p style=""margin:0; color:#94a3b8; font-size:12px;"">© 2025 eBay Clone Group 4 – PRN232. Email này được gửi tự động, vui lòng không trả lời.</p>
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
        }
    }
}
