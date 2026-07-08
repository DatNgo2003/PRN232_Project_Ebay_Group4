using Backend.Repositories.Implementation;
using Backend.Repositories.Interface;
using Backend.Services.Implementation;
using Backend.Services.Interface;

namespace Backend.ProgramConfig
{
    public static class Tan
	{
		public static IServiceCollection AddMyServices3(this IServiceCollection services)
		{
            services.AddScoped<ISellerToBuyerReviewRepository, SellerToBuyerReviewRepository>();
            services.AddScoped<ISellerToBuyerReviewService, SellerToBuyerReviewService>();

            return services;
		}
	}
}
