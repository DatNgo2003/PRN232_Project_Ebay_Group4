namespace Backend.DTOs.Requests
{
    /// <summary>
    /// Request DTO để cập nhật trạng thái giao hàng của đơn hàng.
    /// </summary>
    public class UpdateShippingStatusRequestDto
    {
        /// <summary>
        /// Trạng thái giao hàng mới. Các giá trị hợp lệ: Preparing | Shipping | Delivered | Failed
        /// </summary>
        public string Status { get; set; } = string.Empty;
    }
}
