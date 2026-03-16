using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Qtemplate.Application.Constants;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Services.Cleanup;

public class NotificationCleanupService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<NotificationCleanupService> _logger;

    public NotificationCleanupService(IServiceProvider services, ILogger<NotificationCleanupService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("NotificationCleanupService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var delay = ComputeDelayUntil(hour: 4, minute: 0); // 04:00 UTC
                _logger.LogInformation(
                    "NotificationCleanupService: next run in {Minutes:F0} minutes.", delay.TotalMinutes);

                await Task.Delay(delay, stoppingToken);
                await ProcessAsync();
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NotificationCleanupService error.");
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }

        _logger.LogInformation("NotificationCleanupService stopped.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    private async Task ProcessAsync()
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var settingRepo = scope.ServiceProvider.GetRequiredService<ISettingRepository>();

        int retentionDays = await settingRepo.GetIntAsync(
            SettingKeys.NotificationRetentionDays, defaultValue: 30);

        var cutoff = DateTime.UtcNow.AddDays(-retentionDays);

        // Chỉ xóa notification đã đọc — chưa đọc giữ lại dù lâu
        int deleted = await db.Notifications
            .Where(n => n.IsRead && n.CreatedAt < cutoff)
            .ExecuteDeleteAsync();

        if (deleted > 0)
            _logger.LogInformation(
                "NotificationCleanupService: deleted {Count} read notifications (retention={Days}d, cutoff={Cutoff:yyyy-MM-dd}).",
                deleted, retentionDays, cutoff);
        else
            _logger.LogDebug("NotificationCleanupService: nothing to delete.");
    }

    // ─────────────────────────────────────────────────────────────────────────
    private static TimeSpan ComputeDelayUntil(int hour, int minute)
    {
        var now = DateTime.UtcNow;
        var target = now.Date.AddHours(hour).AddMinutes(minute);
        if (target <= now) target = target.AddDays(1);
        return target - now;
    }
}