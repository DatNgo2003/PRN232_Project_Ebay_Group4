using Backend.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Backend.Services
{
    public class DisputeEscalationService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<DisputeEscalationService> _logger;
        private readonly IConfiguration _configuration;

        public DisputeEscalationService(
            IServiceProvider services,
            ILogger<DisputeEscalationService> logger,
            IConfiguration configuration)
        {
            _services = services;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var daysToEscalate = _configuration.GetValue<int>("DisputeSettings:AutoEscalateDays", 3);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _services.CreateScope())
                    {
                        var disputeRepo = scope.ServiceProvider.GetRequiredService<IDisputeRepository>();
                        await disputeRepo.AutoEscalateDisputesAsync(daysToEscalate);
                    }

                    _logger.LogInformation("Checked disputes for auto escalation at {time}", DateTimeOffset.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while auto escalating disputes");
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
