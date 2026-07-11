namespace Backend.Services
{
    public interface IEmailService
    {
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
