using Backend.Repositories.Implementation;
using Backend.Repositories.Interface;
using Backend.Services.Implementation;
using Backend.Services.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace Backend.ProgramConfig
{
    public static class Quan
    {
        public static IServiceCollection AddMyServices4(this IServiceCollection services)
        {




            services.AddScoped<IDisputeRepositoryV2, DisputeRepositoryV2>();
            services.AddScoped<IDisputeServiceV2, DisputeServiceV2>();
            services.AddRateLimiter(options =>
            {
                // Giới hạn CHUNG cho toàn bộ API, tính theo IP (áp dụng mặc định
                // qua MapControllers().RequireRateLimiting("fixed_by_ip") trong Program.cs).
                options.AddSlidingWindowLimiter(policyName: "fixed_by_ip", opt =>
                {
                    opt.PermitLimit = 50; // 50 requests
                    opt.Window = TimeSpan.FromMinutes(1); // trong 1 phút
                    opt.SegmentsPerWindow = 6;

                    opt.QueueLimit = 0;
                });


                options.AddPolicy("fixed_by_user", httpContext =>
                {
                    var userId = httpContext.User.Identity?.Name ?? "anonymous";

                    return RateLimitPartition.GetFixedWindowLimiter(partitionKey: userId, factory: _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 50, // 50 requests
                            Window = TimeSpan.FromMinutes(1)
                        });
                });

                // >>> MỚI: Giới hạn RIÊNG và CHẶT HƠN cho nhóm API Payment/Shipping
                // (quick-buy, PayPal create/capture, cập nhật trạng thái giao hàng, webhook vận chuyển).
                // Đây là các thao tác "đắt" (gọi PayPal API thật, tạo vận đơn, gửi email) nên cần
                // giới hạn riêng để chống spam/abuse, tách biệt khỏi giới hạn chung của toàn hệ thống.
                // - Nếu người dùng đã đăng nhập: giới hạn theo username (chặn 1 tài khoản spam).
                // - Nếu là hệ thống ngoài gọi vào (webhook, chưa đăng nhập): giới hạn theo IP.
                options.AddPolicy("payment_shipping", httpContext =>
                {
                    var partitionKey = httpContext.User?.Identity?.IsAuthenticated == true
                        ? $"user:{httpContext.User.Identity!.Name}"
                        : $"ip:{httpContext.Connection.RemoteIpAddress}";

                    return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ =>
                        new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 20, // tối đa 20 request
                            Window = TimeSpan.FromMinutes(1), // trong 1 phút
                            QueueLimit = 0
                        });
                });

                // Trả về lỗi 429 khi bị giới hạn
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                // >>> MỚI: trả JSON rõ ràng + header Retry-After + log lại request bị chặn
                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.ContentType = "application/json";

                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        context.HttpContext.Response.Headers.RetryAfter =
                            ((int)retryAfter.TotalSeconds).ToString();
                    }

                    var logger = context.HttpContext.RequestServices.GetService<ILogger<Program>>();
                    logger?.LogWarning(
                        "Rate limit vượt ngưỡng | Path={Path} | IP={IP} | User={User}",
                        context.HttpContext.Request.Path,
                        context.HttpContext.Connection.RemoteIpAddress,
                        context.HttpContext.User.Identity?.Name ?? "anonymous");

                    await context.HttpContext.Response.WriteAsync(
                        JsonSerializer.Serialize(new
                        {
                            message = "Bạn đã gửi quá nhiều yêu cầu. Vui lòng thử lại sau ít phút."
                        }),
                        cancellationToken);
                };
            });
            return services;
        }
    }
}