using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Qtemplate.Application.Constants;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Infrastructure.Services.Security;

public class SuspiciousBehaviorBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<SuspiciousBehaviorBackgroundService> _logger;

    public SuspiciousBehaviorBackgroundService(
        IServiceProvider services,
        ILogger<SuspiciousBehaviorBackgroundService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Suspicious Behavior Scanner started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var settingRepo = scope.ServiceProvider.GetRequiredService<ISettingRepository>();
                var scanner = scope.ServiceProvider.GetRequiredService<SuspiciousBehaviorScanner>();

                int intervalMinutes = await settingRepo.GetIntAsync(
                    SettingKeys.SecurityScanIntervalMinutes, defaultValue: 5);

                await scanner.RunAsync(stoppingToken);

                _logger.LogInformation("Scan complete. Next run in {Min} min.", intervalMinutes);
                await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Suspicious Behavior Scanner error. Retry in 1 minute.");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        _logger.LogInformation("Suspicious Behavior Scanner stopped.");
    }
}