namespace Backend.Services
{
    public interface IEmailService
    {
        /// <summary>
        /// Sends a confirmation when a payment has been completed successfully.
        /// </summary>
        Task SendPaymentConfirmationEmailAsync(
            string toEmail,
            int orderId,
            decimal totalAmount,
            string? buyerName = null,
            string? paymentMethod = null,
            string? trackingNumber = null,
            IEnumerable<string>? productNames = null);

        /// <summary>
        /// Gửi email thông báo khi trạng thái giao hàng thay đổi (Delivered / Failed)
        /// </summary>
        Task SendShippingStatusEmailAsync(
            string toEmail,
            string buyerName,
            int orderId,
            string shippingStatus,
            IEnumerable<string> productNames,
            string trackingNumber);

        /// <summary>
        /// Gửi email thông báo đơn hàng bị huỷ do quá hạn thanh toán
        /// </summary>
        Task SendOrderCancelledEmailAsync(
            string toEmail,
            string buyerName,
            int orderId,
            IEnumerable<string> productNames,
            decimal totalAmount);
    }
}
