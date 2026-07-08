using Backend.Models;
using Backend.Repositories;
using Backend.Services;
using Backend.Utils;
using Microsoft.EntityFrameworkCore;

namespace Backend.ProgramConfig
{
    public static class Toan
    {
        public static IServiceCollection AddMyServices1(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<CloneEbayDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                }));

            services.AddScoped<JwtUtils>();

            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IReviewRepository, ReviewRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IMessageRepository, MessageRepository>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IReviewService, ReviewService>();

            services.AddScoped<AuthService>();

            return services;
        }
    }
}