using Backend.Repositories;
using Backend.Services;

namespace Backend.ProgramConfig
{
	public static class Vu
	{
		public static IServiceCollection AddMyServices6(this IServiceCollection services)
		{
            services.AddScoped<IDisputeRepository, DisputeRepository>();
            services.AddScoped<IDisputeService, DisputeService>();

            return services;
		}
	}
}
