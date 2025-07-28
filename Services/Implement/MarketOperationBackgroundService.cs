using LocalMartOnline.Services.Interface;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LocalMartOnline.Services.Implement
{
    public class MarketOperationBackgroundService : BackgroundService
    {
        private readonly ILogger<MarketOperationBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _period = TimeSpan.FromMinutes(5); // Check every 5 minutes

        public MarketOperationBackgroundService(
            ILogger<MarketOperationBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Market Operation Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var marketService = scope.ServiceProvider.GetRequiredService<IMarketService>();
                    
                    await marketService.UpdateStoreStatusBasedOnMarketHoursAsync();
                    _logger.LogInformation("Store statuses updated based on market hours at {Time}", DateTimeOffset.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while updating store statuses");
                }

                await Task.Delay(_period, stoppingToken);
            }
        }
    }
}
