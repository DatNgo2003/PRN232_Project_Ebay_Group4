using System.Text;

namespace Frontend.Helpers
{
    /// <summary>
    /// Cung cấp các hàm helper để định dạng hiển thị.
    /// </summary>
    public static class FormattingHelper
    {
        // Bộ ký tự để tạo mã đơn hàng (chỉ chữ hoa và số để dễ đọc)
        private const string OrderNumberChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        /// <summary>
        /// Tạo một mã đơn hàng 12 ký tự ngẫu nhiên giả (nhưng nhất quán)
        /// dựa trên ID thật của đơn hàng.
        /// </summary>
        /// <param name="orderId">ID thật của đơn hàng (dùng làm hạt giống)</param>
        /// <returns>Một chuỗi 12 ký tự ngẫu nhiên.</returns>
        public static string GenerateDisplayOrderNumber(int orderId)
        {
            // Sử dụng OrderId làm "hạt giống" (seed).
            // Điều này đảm bảo rằng Random() sẽ luôn tạo ra CÙNG MỘT CHUỖI
            // "ngẫu nhiên" cho CÙNG MỘT OrderId.
            var random = new Random(orderId);

            var stringBuilder = new StringBuilder(12);

            for (int i = 0; i < 12; i++)
            {
                // Chọn một ký tự ngẫu nhiên từ bộ ký tự
                int index = random.Next(OrderNumberChars.Length);
                stringBuilder.Append(OrderNumberChars[index]);
            }

            // Chia chuỗi 12 ký tự thành 3 nhóm 4 ký tự cho dễ đọc
            // Ví dụ: X5T7-G3P1-Q9A2
            return $"{stringBuilder.ToString(0, 4)}-{stringBuilder.ToString(4, 4)}-{stringBuilder.ToString(8, 4)}";
        }
    }
}