namespace Backend.Exceptions
{
    // Exception nghiệp vụ: coupon hết hạn, hết lượt, không áp dụng được, v.v.
    // Controller bắt riêng exception này để trả 400 thay vì 500.
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message) { }
    }
}