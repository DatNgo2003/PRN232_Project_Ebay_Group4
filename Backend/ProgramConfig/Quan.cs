using Backend.Repositories.Implementation;
using Backend.Repositories.Interface;
using Backend.Services.Implementation;
using Backend.Services.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Text;
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
    
                options.AddSlidingWindowLimiter(policyName: "fixed_by_ip", opt =>
                {
                    opt.PermitLimit = 50; // 100 requests
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

                // Trả về lỗi 429 khi bị giới hạn
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });
            return services;
        }
    }
}
