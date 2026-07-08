using Serilog.Context;

namespace Backend.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private const string _headerKey = "X-Correlation-ID";

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // 1. CỐ GẮNG ĐỌC ID TỪ HEADER
            if (!context.Request.Headers.TryGetValue(_headerKey, out var correlationId))
            {
                // 2. Nếu không có, tạo ID mới (dự phòng)
                correlationId = Guid.NewGuid().ToString();
            }

            // 3. Gán ID vào context để dùng trong ứng dụng
            context.TraceIdentifier = correlationId;

            // 4. Thêm ID vào Response Header
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(_headerKey))
                {
                    context.Response.Headers.Append(_headerKey, correlationId);
                }
                return Task.CompletedTask;
            });

            // 5. QUAN TRỌNG: Đẩy ID vào Serilog LogContext
            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await _next(context);
            }
        }
    }
}
