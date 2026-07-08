namespace Backend.ProgramConfig
{
	public static class Khoa
	{
		public static IServiceCollection AddMyServices5(this IServiceCollection services)
		{
			// Register SignalR
			services.AddSignalR();

			return services;
		}
	}
}
